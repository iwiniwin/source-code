/*
** $Id: lstate.h,v 2.24.1.2 2008/01/03 15:20:39 roberto Exp $
** Global State
** See Copyright Notice in lua.h
*/

#ifndef lstate_h
#define lstate_h

#include "lua.h"

#include "lobject.h"
#include "ltm.h"
#include "lzio.h"



struct lua_longjmp;  /* defined in ldo.c */


/* table of globals */
#define gt(L)	(&L->l_gt)

/* registry */
#define registry(L)	(&G(L)->l_registry)


/* extra stack space to handle TM calls and some other extras */
#define EXTRA_STACK   5


#define BASIC_CI_SIZE           8

#define BASIC_STACK_SIZE        (2*LUA_MINSTACK)


// 字符串表
// 由于全局只有一个，不用太考虑空间利用率
// 所以采用的是开散列算法，即将哈希值相同的对象串在分别独立的链表中，实现起来更为简单
typedef struct stringtable {
  GCObject **hash;  // GCObject *数组的地址，散列桶，桶中存的是相同hash值的TString链表
  lu_int32 nuse;  /* number of elements */  
  int size;  // hash桶大小
} stringtable;


/*
** informations about a call
*/
typedef struct CallInfo {
  StkId base;  /* base for this function */
  StkId func;  /* function index in the stack */
  StkId	top;  /* top for this function */
  const Instruction *savedpc;
  int nresults;  /* expected number of results from this function */
  int tailcalls;  /* number of tail calls lost under this entry */
} CallInfo;



#define curr_func(L)	(clvalue(L->ci->func))
#define ci_func(ci)	(clvalue((ci)->func))
#define f_isLua(ci)	(!ci_func(ci)->c.isC)
#define isLua(ci)	(ttisfunction((ci)->func) && f_isLua(ci))


/*
** `global state', shared by all threads of this state
*/
typedef struct global_State {
  stringtable strt;  /* hash table for strings */  // 全局的字符串表
  lua_Alloc frealloc;  /* function to reallocate memory */  // 申请内存的函数指针
  void *ud;         /* auxiliary data to `frealloc' */
  lu_byte currentwhite;  // 指示当前的白色是0型还是1型
  lu_byte gcstate;  /* state of garbage collector */ // 表示gc处于哪个阶段
  int sweepstrgc;  /* position of sweep in `strt' */
  GCObject *rootgc;  /* list of all collectable objects */  // 出string类型之外的GCObject链表头
  GCObject **sweepgc;  /* position of sweep in `rootgc' */
  GCObject *gray;  /* list of gray objects */
  GCObject *grayagain;  /* list of objects to be traversed atomically */
  GCObject *weak;  /* list of weak tables (to be cleared) */
  GCObject *tmudata;  /* last element of list of userdata to be GC */
  Mbuffer buff;  /* temporary buffer for string concatentation */
  lu_mem GCthreshold;
  lu_mem totalbytes;  /* number of bytes currently allocated */  // lua vm实际分配的字节数
  lu_mem estimate;  /* an estimate of number of bytes actually in use */  // 估算值，lua vm占用的内存字节数
																		  // 比totalbytes要小，因为被检测出已经访问不到的userdata占用的内存不会马上释放（保证gc元方法的安全调用）
																		  // 但estimate会抛去这部分，见lgc.c 553行 g->estimate = g->totalbytes - udsize;
  lu_mem gcdept;  /* how much GC is `behind schedule' */
  int gcpause;  /* size of pause between successive GCs */
  int gcstepmul;  /* GC `granularity' */
  lua_CFunction panic;  /* to be called in unprotected errors */
  TValue l_registry;
  struct lua_State *mainthread;
  UpVal uvhead;  /* head of double-linked list of all open upvalues */
  struct Table *mt[NUM_TAGS];  /* metatables for basic types */
  TString *tmname[TM_N];  /* array with tag-method names */  // 以TString指针的方式记录所有元方法的名字  
															 // lua的gc算法并不做内存整理，它不会在内存迁移数据，所以如果一个string肯定不会被清除
															 // 那么它的内存地址也是不变的
} global_State;


/*
** `per thread' state
** 表示lua虚拟机线程的状态
** 完整的lua虚拟机运行时，可能有多个lua_State，即多个线程
*/
struct lua_State {
  CommonHeader;
  lu_byte status;
  StkId top;  /* first free slot in the stack */
  StkId base;  /* base of current function */
  global_State *l_G;  // 多个线程共享的数据放在global_State域
  CallInfo *ci;  /* call info for current function */
  const Instruction *savedpc;  /* `savedpc' of current function */
  StkId stack_last;  /* last free slot in the stack */
  StkId stack;  /* stack base */
  CallInfo *end_ci;  /* points after end of ci array*/
  CallInfo *base_ci;  /* array of CallInfo's */
  int stacksize;
  int size_ci;  /* size of array `base_ci' */
  unsigned short nCcalls;  /* number of nested C calls */
  unsigned short baseCcalls;  /* nested C calls when resuming coroutine */
  lu_byte hookmask;
  lu_byte allowhook;
  int basehookcount;
  int hookcount;
  lua_Hook hook;
  TValue l_gt;  /* table of globals */
  TValue env;  /* temporary place for environments */
  GCObject *openupval;  /* list of open upvalues in this stack */
  GCObject *gclist;
  struct lua_longjmp *errorJmp;  /* current error recover point */
  ptrdiff_t errfunc;  /* current error handling function (stack index) */
};

// lua_State - > global_State
#define G(L)	(L->l_G)


/*
** Union of all collectable objects
*/
union GCObject {
  GCheader gch;         /* common header */
  union TString ts;     // 字符串
  union Udata u;        // userdata
  union Closure cl;     // 闭包
  struct Table h;       // 表
  struct Proto p;       // proto
  struct UpVal uv;      // upvalue
  struct lua_State th;  /* thread */
};


/* macros to convert a GCObject into a specific value */
// GCObject -> TString
#define rawgco2ts(o)	check_exp((o)->gch.tt == LUA_TSTRING, &((o)->ts))
// GCObject -> TString - > tsv struct
#define gco2ts(o)	(&rawgco2ts(o)->tsv)
#define rawgco2u(o)	check_exp((o)->gch.tt == LUA_TUSERDATA, &((o)->u))
#define gco2u(o)	(&rawgco2u(o)->uv)
#define gco2cl(o)	check_exp((o)->gch.tt == LUA_TFUNCTION, &((o)->cl))
#define gco2h(o)	check_exp((o)->gch.tt == LUA_TTABLE, &((o)->h))
#define gco2p(o)	check_exp((o)->gch.tt == LUA_TPROTO, &((o)->p))
#define gco2uv(o)	check_exp((o)->gch.tt == LUA_TUPVAL, &((o)->uv))
#define ngcotouv(o) \
	check_exp((o) == NULL || (o)->gch.tt == LUA_TUPVAL, &((o)->uv))
#define gco2th(o)	check_exp((o)->gch.tt == LUA_TTHREAD, &((o)->th))

/* macro to convert any Lua object into a GCObject */
// 将LuaObject转换成GCObject
#define obj2gco(v)	(cast(GCObject *, (v)))


LUAI_FUNC lua_State *luaE_newthread (lua_State *L);
LUAI_FUNC void luaE_freethread (lua_State *L, lua_State *L1);

#endif

