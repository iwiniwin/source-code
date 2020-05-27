/*
** $Id: lstring.c,v 2.8.1.1 2007/12/27 13:02:25 roberto Exp $
** String table (keeps all strings handled by Lua)
** See Copyright Notice in lua.h
*/


#include <string.h>

#define lstring_c
#define LUA_CORE

#include "lua.h"

#include "lmem.h"
#include "lobject.h"
#include "lstate.h"
#include "lstring.h"


// 对保存字符串的哈希桶进行resize
void luaS_resize (lua_State *L, int newsize) {
  GCObject **newhash;
  stringtable *tb;
  int i;
  if (G(L)->gcstate == GCSsweepstring)  // 垃圾回收正在回收字符串，不进行resize
    return;  /* cannot resize during GC traverse */
  newhash = luaM_newvector(L, newsize, GCObject *);
  tb = &G(L)->strt;
  for (i=0; i<newsize; i++) newhash[i] = NULL;
  /* rehash */
  for (i=0; i<tb->size; i++) {
    GCObject *p = tb->hash[i];
    while (p) {  /* for each node in the list */
      GCObject *next = p->gch.next;  /* save next */
      unsigned int h = gco2ts(p)->hash;
      int h1 = lmod(h, newsize);  /* new position */
      lua_assert(cast_int(h%newsize) == lmod(h, newsize));
      p->gch.next = newhash[h1];  /* chain it */
      newhash[h1] = p;
      p = next;
    }
  }
  luaM_freearray(L, tb->hash, tb->size, TString *);
  tb->size = newsize;
  tb->hash = newhash;
}

// 申请新的字符串
static TString *newlstr (lua_State *L, const char *str, size_t l,
                                       unsigned int h) {
  TString *ts;
  stringtable *tb;
  if (l+1 > (MAX_SIZET - sizeof(TString))/sizeof(char))  // 字符串过大
    luaM_toobig(L);
  ts = cast(TString *, luaM_malloc(L, (l+1)*sizeof(char)+sizeof(TString)));
  ts->tsv.len = l;  // 设置字符串长度
  ts->tsv.hash = h;  // 设置字符串hash值
  ts->tsv.marked = luaC_white(G(L));
  ts->tsv.tt = LUA_TSTRING;  // 设置字符串类型
  ts->tsv.reserved = 0;
  memcpy(ts+1, str, l*sizeof(char));  // 拷贝字符串，从 str 复制 l*sizeof(char) 个字节到 ts+1
  ((char *)(ts+1))[l] = '\0';  /* ending 0 */  // 按照c风格存放，以兼容c接口
  tb = &G(L)->strt;
  h = lmod(h, tb->size);  // 对hash值求余作为索引
  ts->tsv.next = tb->hash[h];  /* chain new entry */
  tb->hash[h] = obj2gco(ts);  // 将新创建的字符串添加到全局字符串表中
  tb->nuse++;
  // 如果全局字符串表中的元素数量超过了hash桶数组大小，如果不resize会发生冲突
  // 并且桶数量未超过MAX_INT的一半，就成倍扩充
  if (tb->nuse > cast(lu_int32, tb->size) && tb->size <= MAX_INT/2)
    luaS_resize(L, tb->size*2);  /* too crowded */
  return ts;
}

// 申请新的字符串
TString *luaS_newlstr (lua_State *L, const char *str, size_t l) {
  GCObject *o;
  unsigned int h = cast(unsigned int, l);  /* seed */
  size_t step = (l>>5)+1;  /* if string is too long, don't hash all its chars */  // 如果字符串太长就不逐字符比较，增大step
  size_t l1;
  for (l1=l; l1>=step; l1-=step)  /* compute hash */
    h = h ^ ((h<<5)+(h>>2)+cast(unsigned char, str[l1-1]));  // 计算hash值
  for (o = G(L)->strt.hash[lmod(h, G(L)->strt.size)];  // 在global_State的stringtable结构的hash表中查找字符串是否已经有了
       o != NULL;
       o = o->gch.next) {
    TString *ts = rawgco2ts(o);
	// 比较是否是目标字符串，memcmp比较内存的前l个字节
    if (ts->tsv.len == l && (memcmp(str, getstr(ts), l) == 0)) {
      /* string may be dead */
	  // 由于lua的垃圾收集过程是分步完成的，而添加新字符串在任何步骤之间都有可能发生
      if (isdead(G(L), o)) changewhite(o); // 如果是死的字符串，则恢复白的身份
      return ts;
    }
  }
  // 未在全局表中找到，申请新字符串
  return newlstr(L, str, l, h);  /* not found */
}

// 申请新的userdata
// userdata在存储形式上和字符串相似，可以看成是拥有独立元表，不被内部化处理，也不需要追加\0的字符串
Udata *luaS_newudata (lua_State *L, size_t s, Table *e) {
  Udata *u;
  if (s > MAX_SIZET - sizeof(Udata))
    luaM_toobig(L);
  u = cast(Udata *, luaM_malloc(L, s + sizeof(Udata)));
  u->uv.marked = luaC_white(G(L));  /* is not finalized */
  u->uv.tt = LUA_TUSERDATA;
  u->uv.len = s;
  u->uv.metatable = NULL;
  u->uv.env = e;
  /* chain it on udata list (after main thread) */
  u->uv.next = G(L)->mainthread->next;
  G(L)->mainthread->next = obj2gco(u);
  return u;
}

