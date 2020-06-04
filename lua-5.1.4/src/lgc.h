/*
** $Id: lgc.h,v 2.15.1.1 2007/12/27 13:02:25 roberto Exp $
** Garbage Collector
** See Copyright Notice in lua.h
*/

#ifndef lgc_h
#define lgc_h


#include "lobject.h"


/*
** Possible states of the Garbage Collector
** 状态值的大小也表示了它们的执行顺序
*/
#define GCSpause	0  // 每个gc流程的起始步骤，只是标记系统的根节点
#define GCSpropagate	1  // 标记流程
#define GCSsweepstring	2  // 回收字符串状态
#define GCSsweep	3  // 回收除字符串外的其他类型
#define GCSfinalize	4  // 此阶段处理需要调用gc方法的userdata对象


/*
** some userful bit tricks
*/
#define resetbits(x,m)	((x) &= cast(lu_byte, ~(m)))
#define setbits(x,m)	((x) |= (m))
#define testbits(x,m)	((x) & (m))
#define bitmask(b)	(1<<(b))
#define bit2mask(b1,b2)	(bitmask(b1) | bitmask(b2))
#define l_setbit(x,b)	setbits(x, bitmask(b))
#define resetbit(x,b)	resetbits(x, bitmask(b))
#define testbit(x,b)	testbits(x, bitmask(b))
#define set2bits(x,b1,b2)	setbits(x, (bit2mask(b1, b2)))
#define reset2bits(x,b1,b2)	resetbits(x, (bit2mask(b1, b2)))
#define test2bits(x,b1,b2)	testbits(x, (bit2mask(b1, b2)))



/*
** Layout for bit use in `marked' field:
** bit 0 - object is white (type 0)  // 一开始所有节点都是白色的，新创建出来的节点也被默认设置为白色
** bit 1 - object is white (type 1)  // 当处于清理流程中时，如果对象发生了变化，比如增加了一个对象，安全的方法是将其标记为不可清理即黑色
									 // 但清理过程结束后需要把所有对象置回白色，lua实际上是单遍扫描，处理完一个节点重置一个节点的颜色
									 // 所以如果简单的把对象设置为黑色，可能导致它再也没机会变回白色，所以引入第三种状态white1
** bit 2 - object is black
** bit 3 - for userdata: has been finalized  // 用于标记userdata，当userdata确认不被引用，则设置上这个标记
											 // 它与颜色标记不同，是用于保证userdata的gc元方法不被反复调用
** bit 3 - for tables: has weak keys  // 标记table的weak属性
** bit 4 - for tables: has weak values  // 标记table的weak属性
** bit 5 - object is fixed (should not be collected)  // 保证一个GCObject不会再GC过程中被清除，为什么要有这种状态？
													  // lua本身会用到一个字符串，它们可能不被任何地方引用，但又希望这个字符串反复生成，通过设置fixed保护这个字符串
** bit 6 - object is "super" fixed (only the main thread)  // 专门用于标记mainthread，一切的起点
*/


#define WHITE0BIT	0
#define WHITE1BIT	1
#define BLACKBIT	2
#define FINALIZEDBIT	3  
#define KEYWEAKBIT	3
#define VALUEWEAKBIT	4
#define FIXEDBIT	5
#define SFIXEDBIT	6
#define WHITEBITS	bit2mask(WHITE0BIT, WHITE1BIT)


#define iswhite(x)      test2bits((x)->gch.marked, WHITE0BIT, WHITE1BIT)
#define isblack(x)      testbit((x)->gch.marked, BLACKBIT)
#define isgray(x)	(!isblack(x) && !iswhite(x))

// 乒乓切换，取另外一种白色，如果是white0就返回white1，是white1就返回white0
#define otherwhite(g)	(g->currentwhite ^ WHITEBITS)

// 如果节点的白色是otherwhite，那么就是一个死节点
// 这个函数是在mark阶段过后使用的，所以此时的otherwhite其实就是本次GC的白色
#define isdead(g,v)	((v)->gch.marked & otherwhite(g) & WHITEBITS)

#define changewhite(x)	((x)->gch.marked ^= WHITEBITS)
#define gray2black(x)	l_setbit((x)->gch.marked, BLACKBIT)

#define valiswhite(x)	(iscollectable(x) && iswhite(gcvalue(x)))

// 获取当前的白色类型
#define luaC_white(g)	cast(lu_byte, (g)->currentwhite & WHITEBITS)

// 用于自动GC
// 保证GC可以随内存使用增加而自动进行
#define luaC_checkGC(L) { \
  condhardstacktests(luaD_reallocstack(L, L->stacksize - EXTRA_STACK - 1)); \
  if (G(L)->totalbytes >= G(L)->GCthreshold) \
	luaC_step(L); }


#define luaC_barrier(L,p,v) { if (valiswhite(v) && isblack(obj2gco(p)))  \
	luaC_barrierf(L,obj2gco(p),gcvalue(v)); }

#define luaC_barriert(L,t,v) { if (valiswhite(v) && isblack(obj2gco(t)))  \
	luaC_barrierback(L,t); }

#define luaC_objbarrier(L,p,o)  \
	{ if (iswhite(obj2gco(o)) && isblack(obj2gco(p))) \
		luaC_barrierf(L,obj2gco(p),obj2gco(o)); }

#define luaC_objbarriert(L,t,o)  \
   { if (iswhite(obj2gco(o)) && isblack(obj2gco(t))) luaC_barrierback(L,t); }

LUAI_FUNC size_t luaC_separateudata (lua_State *L, int all);
LUAI_FUNC void luaC_callGCTM (lua_State *L);
LUAI_FUNC void luaC_freeall (lua_State *L);
LUAI_FUNC void luaC_step (lua_State *L);
LUAI_FUNC void luaC_fullgc (lua_State *L);
LUAI_FUNC void luaC_link (lua_State *L, GCObject *o, lu_byte tt);
LUAI_FUNC void luaC_linkupval (lua_State *L, UpVal *uv);
LUAI_FUNC void luaC_barrierf (lua_State *L, GCObject *o, GCObject *v);
LUAI_FUNC void luaC_barrierback (lua_State *L, Table *t);


#endif
