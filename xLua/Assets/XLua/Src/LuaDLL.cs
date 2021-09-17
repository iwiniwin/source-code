/*
 * Tencent is pleased to support the open source community by making xLua available.
 * Copyright (C) 2016 THL A29 Limited, a Tencent company. All rights reserved.
 * Licensed under the MIT License (the "License"); you may not use this file except in compliance with the License. You may obtain a copy of the License at
 * http://opensource.org/licenses/MIT
 * Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific language governing permissions and limitations under the License.
*/

namespace XLua.LuaDLL
{

    using System;
    using System.Runtime.InteropServices;
    using System.Text;
    using XLua;

#if UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN || XLUA_GENERAL || (UNITY_WSA && !UNITY_EDITOR)
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate int lua_CSFunction(IntPtr L);

#if GEN_CODE_MINIMIZE
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate int CSharpWrapperCaller(IntPtr L, int funcidx, int top);
#endif
#else
    public delegate int lua_CSFunction(IntPtr L);

#if GEN_CODE_MINIMIZE
    public delegate int CSharpWrapperCaller(IntPtr L, int funcidx, int top);
#endif
#endif


    public partial class Lua
	{
#if (UNITY_IPHONE || UNITY_TVOS || UNITY_WEBGL || UNITY_SWITCH) && !UNITY_EDITOR
        const string LUADLL = "__Internal";
#else
        const string LUADLL = "xlua";
#endif

        [DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr lua_tothread(IntPtr L, int index);

        [DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl)]
		public static extern int xlua_get_lib_version();

        [DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern int lua_gc(IntPtr L, LuaGCOptions what, int data);

        [DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr lua_getupvalue(IntPtr L, int funcindex, int n);

        [DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr lua_setupvalue(IntPtr L, int funcindex, int n);

        // 把 {L} 表示的线程压栈。 如果这个线程是当前状态机的主线程的话，返回 1
        [DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl)]
		public static extern int lua_pushthread(IntPtr L);

		public static bool lua_isfunction(IntPtr L, int stackPos)
		{
			return lua_type(L, stackPos) == LuaTypes.LUA_TFUNCTION;
		}

		public static bool lua_islightuserdata(IntPtr L, int stackPos)
		{
			return lua_type(L, stackPos) == LuaTypes.LUA_TLIGHTUSERDATA;
		}

		public static bool lua_istable(IntPtr L, int stackPos)
		{
			return lua_type(L, stackPos) == LuaTypes.LUA_TTABLE;
		}

		public static bool lua_isthread(IntPtr L, int stackPos)
		{
			return lua_type(L, stackPos) == LuaTypes.LUA_TTHREAD;
		}

        public static int luaL_error(IntPtr L, string message) //[-0, +1, m]
        {
            xlua_csharp_str_error(L, message);
            return 0;
        }

		[DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl)]
		public static extern int lua_setfenv(IntPtr L, int stackPos);

        [DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl)]
		public static extern IntPtr luaL_newstate();

		[DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl)]
		public static extern void lua_close(IntPtr L);

		[DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl)] //[-0, +0, m]
        public static extern void luaopen_xlua(IntPtr L);

        [DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl)] //[-0, +0, m]
        public static extern void luaL_openlibs(IntPtr L);

        [DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern uint xlua_objlen(IntPtr L, int stackPos);

        // 创建一张新的空表压栈。 参数 narr 建议了这张表作为序列使用时会有多少个元素； 参数 nrec 建议了这张表可能拥有多少序列之外的元素。 Lua 会使用这些建议来预分配这张新表。 如果你知道这张表用途的更多信息，预分配可以提高性能。 否则，你可以使用函数 lua_newtable 。
		[DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl)]
		public static extern void lua_createtable(IntPtr L, int narr, int nrec);//[-0, +0, m]

        // 创建一个空表，并将其压栈
        public static void lua_newtable(IntPtr L)//[-0, +0, m]
        {
			lua_createtable(L, 0, 0);
		}

        // 把全局变量 {name} 里的值压栈，返回该值的类型
        [DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern int xlua_getglobal(IntPtr L, string name);//[-1, +0, m]

        [DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern int xlua_setglobal(IntPtr L, string name);//[-1, +0, m]

        [DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern void xlua_getloaders(IntPtr L);

        [DllImport(LUADLL,CallingConvention=CallingConvention.Cdecl)]
		public static extern void lua_settop(IntPtr L, int newTop);

		public static void lua_pop(IntPtr L, int amount)
		{
			lua_settop(L, -(amount) - 1);
		}
		[DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl)]
		public static extern void lua_insert(IntPtr L, int newTop);

        // 从给定有效index处移除一个元素， 把这个索引之上的所有元素移下来填补上这个空隙。 不能用伪索引来调用这个函数，因为伪索引并不指向真实的栈上的位置。
		[DllImport(LUADLL,CallingConvention=CallingConvention.Cdecl)]
		public static extern void lua_remove(IntPtr L, int index);

        // 把 t[k] 的值压栈， 这里的 t 是指索引index指向的值， 而 k 则是栈顶放的值。这个函数会弹出堆栈上的键，把结果放在栈上相同位置。
        [DllImport(LUADLL,CallingConvention=CallingConvention.Cdecl)]
		public static extern int lua_rawget(IntPtr L, int index);

        // 不触发元方法赋值t[k] = v，函数完成后会将k,v弹出。t是{index}处的表，v是栈顶值，k是栈顶之下的值
        [DllImport(LUADLL,CallingConvention=CallingConvention.Cdecl)]
		public static extern void lua_rawset(IntPtr L, int index);//[-2, +0, m]

        // 把一张表弹出栈，并将其设为{objIndex}处的值的元表。
        [DllImport(LUADLL,CallingConvention=CallingConvention.Cdecl)]
		public static extern int lua_setmetatable(IntPtr L, int objIndex);

        [DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern int lua_rawequal(IntPtr L, int index1, int index2);

        // 把栈上{index}处的元素作一个副本压栈。
        [DllImport(LUADLL,CallingConvention=CallingConvention.Cdecl)]
		public static extern void lua_pushvalue(IntPtr L, int index);

        // 把一个新的 C 闭包压栈。参数 n 告之函数有多少个值需要关联到函数上。 lua_pushcclosure 也会把这些值从栈上弹出。
        [DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern void lua_pushcclosure(IntPtr L, IntPtr fn, int n);//[-n, +1, m]

        [DllImport(LUADLL,CallingConvention=CallingConvention.Cdecl)]
		public static extern void lua_replace(IntPtr L, int index);

        // 返回栈顶元素的索引。 因为索引是从 1 开始编号的， 所以这个结果等于栈上的元素个数； 特别指出，0 表示栈为空。
		[DllImport(LUADLL,CallingConvention=CallingConvention.Cdecl)]
		public static extern int lua_gettop(IntPtr L);

        // 返回给定{index}处值的类型， 当索引无效（或无法访问）时则返回 LUA_TNONE
		[DllImport(LUADLL,CallingConvention=CallingConvention.Cdecl)]
		public static extern LuaTypes lua_type(IntPtr L, int index);

		public static bool lua_isnil(IntPtr L, int index)
		{
			return (lua_type(L,index)==LuaTypes.LUA_TNIL);
		}

		[DllImport(LUADLL,CallingConvention=CallingConvention.Cdecl)]
		public static extern bool lua_isnumber(IntPtr L, int index);

		public static bool lua_isboolean(IntPtr L, int index)
		{
			return lua_type(L,index)==LuaTypes.LUA_TBOOLEAN;
		}

        // 针对栈顶的对象，创建并返回一个在索引 registryIndex 指向的表中的 引用 （最后会弹出栈顶对象）
		[DllImport(LUADLL,CallingConvention=CallingConvention.Cdecl)]
		public static extern int luaL_ref(IntPtr L, int registryIndex);

        public static int luaL_ref(IntPtr L)//[-1, +0, m]
        {
			return luaL_ref(L,LuaIndexes.LUA_REGISTRYINDEX);
		}

        // 把 t[index] 的值压栈， 这里的 t 是指给定{tableIndex}处的表。 这是一次直接访问；就是说，它不会触发元方法。
		[DllImport(LUADLL,CallingConvention=CallingConvention.Cdecl)]
		public static extern void xlua_rawgeti(IntPtr L, int tableIndex, long index);

        // 等价于 t[index] = v ， 这里的 t 是指给定tableIndex处的表， 而 v 是栈顶的值。这个函数会将值弹出栈。
		[DllImport(LUADLL,CallingConvention=CallingConvention.Cdecl)]
		public static extern void xlua_rawseti(IntPtr L, int tableIndex, long index);//[-1, +0, m]

        // 获取注册表reference处索引的值
        public static void lua_getref(IntPtr L, int reference)
		{
			xlua_rawgeti(L,LuaIndexes.LUA_REGISTRYINDEX,reference);
		}

        [DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern int pcall_prepare(IntPtr L, int error_func_ref, int func_ref);

        [DllImport(LUADLL,CallingConvention=CallingConvention.Cdecl)]
		public static extern void luaL_unref(IntPtr L, int registryIndex, int reference);

		public static void lua_unref(IntPtr L, int reference)
		{
			luaL_unref(L,LuaIndexes.LUA_REGISTRYINDEX,reference);
		}

		[DllImport(LUADLL,CallingConvention=CallingConvention.Cdecl)]
		public static extern bool lua_isstring(IntPtr L, int index);

        [DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern bool lua_isinteger(IntPtr L, int index);

        [DllImport(LUADLL,CallingConvention=CallingConvention.Cdecl)]
		public static extern void lua_pushnil(IntPtr L);

        // push一个lua_CSFunction，n表示要为闭包关联的值的个数
		public static void lua_pushstdcallcfunction(IntPtr L, lua_CSFunction function, int n = 0)//[-0, +1, m]
        {
#if XLUA_GENERAL || (UNITY_WSA && !UNITY_EDITOR)
            GCHandle.Alloc(function);
#endif
            IntPtr fn = Marshal.GetFunctionPointerForDelegate(function);  // 将委托转换为可从非托管代码调用的函数指针
            xlua_push_csharp_function(L, fn, n);
        }

        [DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern int xlua_upvalueindex(int n);

        [DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern int lua_pcall(IntPtr L, int nArgs, int nResults, int errfunc);

        [DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern double lua_tonumber(IntPtr L, int index);

        [DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern int xlua_tointeger(IntPtr L, int index);

        [DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern uint xlua_touint(IntPtr L, int index);

        [DllImport(LUADLL,CallingConvention=CallingConvention.Cdecl)]
		public static extern bool lua_toboolean(IntPtr L, int index);

        [DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr lua_topointer(IntPtr L, int index);

        [DllImport(LUADLL,CallingConvention = CallingConvention.Cdecl)]
		public static extern IntPtr lua_tolstring(IntPtr L, int index, out IntPtr strLen);//[-0, +0, m]

        public static string lua_tostring(IntPtr L, int index)
		{
            IntPtr strlen;

            IntPtr str = lua_tolstring(L, index, out strlen);
            if (str != IntPtr.Zero)
			{
#if XLUA_GENERAL || (UNITY_WSA && !UNITY_EDITOR)
                int len = strlen.ToInt32();
                byte[] buffer = new byte[len];
                Marshal.Copy(str, buffer, 0, len);
                return Encoding.UTF8.GetString(buffer);
#else
                string ret = Marshal.PtrToStringAnsi(str, strlen.ToInt32());
                if (ret == null)
                {
                    int len = strlen.ToInt32();
                    byte[] buffer = new byte[len];
                    Marshal.Copy(str, buffer, 0, len);
                    return Encoding.UTF8.GetString(buffer);
                }
                return ret;
#endif
            }
            else
			{
                return null;
			}
		}

        [DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl)]
		public static extern IntPtr lua_atpanic(IntPtr L, lua_CSFunction panicf);

		[DllImport(LUADLL,CallingConvention=CallingConvention.Cdecl)]
		public static extern void lua_pushnumber(IntPtr L, double number);

		[DllImport(LUADLL,CallingConvention=CallingConvention.Cdecl)]
		public static extern void lua_pushboolean(IntPtr L, bool value);

        [DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern void xlua_pushinteger(IntPtr L, int value);

        [DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern void xlua_pushuint(IntPtr L, uint value);

#if NATIVE_LUA_PUSHSTRING
        [DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern void lua_pushstring(IntPtr L, string str);
#else
        public static void lua_pushstring(IntPtr L, string str) //业务使用
        {
            if (str == null)
            {
                lua_pushnil(L);
            }
            else
            {
#if !THREAD_SAFE && !HOTFIX_ENABLE
                if (Encoding.UTF8.GetByteCount(str) > InternalGlobals.strBuff.Length)
                {
                    byte[] bytes = Encoding.UTF8.GetBytes(str);
                    xlua_pushlstring(L, bytes, bytes.Length);
                }
                else
                {
                    int bytes_len = Encoding.UTF8.GetBytes(str, 0, str.Length, InternalGlobals.strBuff, 0);
                    xlua_pushlstring(L, InternalGlobals.strBuff, bytes_len);
                }
#else
                var bytes = Encoding.UTF8.GetBytes(str);
                xlua_pushlstring(L, bytes, bytes.Length);
#endif
            }
        }
#endif

        [DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern void xlua_pushlstring(IntPtr L, byte[] str, int size);

        // 压入字符串
        public static void xlua_pushasciistring(IntPtr L, string str) // for inner use only
        {
            if (str == null)
            {
                lua_pushnil(L);
            }
            else
            {
#if NATIVE_LUA_PUSHSTRING
                lua_pushstring(L, str);
#else
#if !THREAD_SAFE && !HOTFIX_ENABLE
                int str_len = str.Length;
                if (InternalGlobals.strBuff.Length < str_len)
                {
                    InternalGlobals.strBuff = new byte[str_len];
                }

                int bytes_len = Encoding.UTF8.GetBytes(str, 0, str_len, InternalGlobals.strBuff, 0);
                xlua_pushlstring(L, InternalGlobals.strBuff, bytes_len);
#else
                var bytes = Encoding.UTF8.GetBytes(str);
                xlua_pushlstring(L, bytes, bytes.Length);
#endif
#endif
            }
        }

        public static void lua_pushstring(IntPtr L, byte[] str)
        {
            if (str == null)
            {
                lua_pushnil(L);
            }
            else
            {
                xlua_pushlstring(L, str, str.Length);
            }
        }

        public static byte[] lua_tobytes(IntPtr L, int index)//[-0, +0, m]
        {
            if (lua_type(L, index) == LuaTypes.LUA_TSTRING)
            { 
                IntPtr strlen;
                IntPtr str = lua_tolstring(L, index, out strlen);
                if (str != IntPtr.Zero)
                {
                    int buff_len = strlen.ToInt32();
                    byte[] buffer = new byte[buff_len];
                    Marshal.Copy(str, buffer, 0, buff_len);
                    return buffer;
                }
            }
            return null;
        }

        // 创建一张新表newmetatable = { __name = meta}，并赋值 注册表[meta] = newmetatable
        [DllImport(LUADLL,CallingConvention=CallingConvention.Cdecl)]
		public static extern int luaL_newmetatable(IntPtr L, string meta);//[-0, +1, m]

        // 功能等于lua_gettable，只是以pcall模式运行
        [DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern int xlua_pgettable(IntPtr L, int idx);

        [DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern int xlua_psettable(IntPtr L, int idx);

        // 压栈 注册表[meta]
        public static void luaL_getmetatable(IntPtr L, string meta)
		{
            xlua_pushasciistring(L, meta);
			lua_rawget(L, LuaIndexes.LUA_REGISTRYINDEX);
		}

        [DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern int xluaL_loadbuffer(IntPtr L, byte[] buff, int size, string name);

        public static int luaL_loadbuffer(IntPtr L, string buff, string name)//[-0, +1, m]
        {
            byte[] bytes = Encoding.UTF8.GetBytes(buff);
            return xluaL_loadbuffer(L, bytes, bytes.Length, name);
        }

        // 如果obj索引处是一个userdata，且它有含有xlu_tag标志的元表，则返回userdata指向的内容，即translator.objects中的索引
        // 实际就是校验obj处是否CS对象，是的话就返回该对象，相比xlua_tocsobj_fast多了一层校验
		[DllImport(LUADLL,CallingConvention=CallingConvention.Cdecl)]
		public static extern int xlua_tocsobj_safe(IntPtr L,int obj);//[-0, +0, m]

        // 如果obj索引处是一个userdata，则返回userdata指向的内容，即C#缓存对象的索引
        [DllImport(LUADLL,CallingConvention=CallingConvention.Cdecl)]
		public static extern int xlua_tocsobj_fast(IntPtr L,int obj);

        public static int lua_error(IntPtr L)
        {
            xlua_csharp_error(L);
            return 0;
        }
        // 确保堆栈上至少有 extra 个额外空位
        [DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl)]
		public static extern bool lua_checkstack(IntPtr L,int extra);//[-0, +0, m]

        [DllImport(LUADLL,CallingConvention=CallingConvention.Cdecl)]
		public static extern int lua_next(IntPtr L,int index);//[-1, +(2|0), e]

        [DllImport(LUADLL,CallingConvention=CallingConvention.Cdecl)]
		public static extern void lua_pushlightuserdata(IntPtr L, IntPtr udata);

        // 已注册到lua中的CS类型的标志
 		[DllImport(LUADLL,CallingConvention=CallingConvention.Cdecl)]
		public static extern IntPtr xlua_tag();

        [DllImport(LUADLL,CallingConvention=CallingConvention.Cdecl)]
        public static extern void luaL_where (IntPtr L, int level);//[-0, +1, m]

        [DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern int xlua_tryget_cachedud(IntPtr L, int key, int cache_ref);

        // 真正将对象push到lua的方法，将在lua中创建一个userdata并将其指向key，然后为userdata设置meta_ref的元表，这样该userdata就成为了key表示的CS对象的lua代理
        // key表示C#侧缓存该对象的索引
        // meta_ref表示对象所属类型的元表的索引
        // need_cache表示对象是否需要在lua中进行缓存，如果需要缓存，则在lua缓存表中保存键值对 key = userdata
        // cache_ref表示lua缓存表的索引
        [DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern void xlua_pushcsobj(IntPtr L, int key, int meta_ref, bool need_cache, int cache_ref);//[-0, +1, m]

        /*
        // 创建一个__index函数闭包，__index调用时需要2个参数，obj和key
        // 创建闭包时栈上需要7个关联upvalue
        // [1]: methods, [2]:getters, [3]:csindexer, [4]:base, [5]:indexfuncs, [6]:arrayindexer, [7]:baseindex
        __index索引逻辑
        1. 如果methods中有key，则压栈methods[key]。查询成员方法或事件
        2. 如果getters中有key，则调用getters[key]。查询成员字段或属性
        3. 如果arrayindexer中有key且key是数字，则调用arrayindexer[key]
        4. 尝试调用csindexer。查询索引器
        5. 尝试在父类中查找
            while(base != null)
            {
                if(indexfuncs[base]){
                    baseindex = indexfuncs[base]
                    break;
                }else{
                    base = base["BaseType"]
                }
            }
            如果baseindex不为空，调用baseindex指向的父类__index函数进行查找
        */
        [DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl)]//[,,m]
        public static extern int gen_obj_indexer(IntPtr L);

        /*
        // 创建一个__newindex函数闭包，__newindex调用时需要3个参数，[1]: obj, [2]: key, [3]: value
        // 创建闭包时栈上需要6个关联upvalue
        // [1]:setters, [2]:csnewindexer, [3]:base, [4]:newindexfuncs, [5]:arrayindexer, [6]:basenewindex
        */
        [DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl)]//[,,m]
        public static extern int gen_obj_newindexer(IntPtr L);

        /*
        // 为类的静态域创建一个__index函数闭包，__index调用时需要2个参数，obj和key
        // 创建闭包时栈上需要5个关联upvalue
        // [1]:getters, [2]:feilds, [3]:base, [4]:indexfuncs, [5]:baseindex
        */
        [DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl)]//[,,m]
        public static extern int gen_cls_indexer(IntPtr L);

        /*
        // 为类的静态域创建一个__newindex函数闭包，__newindex调用时需要3个参数，[1]: obj, [2]: key, [3]: value
        // 创建闭包时栈上需要4个关联upvalue
        // [1]:setters, [2]:base, [3]:indexfuncs, [4]:baseindex
        */
        [DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl)]//[,,m]
        public static extern int gen_cls_newindexer(IntPtr L);

        [DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl)]//[,,m]
        public static extern int get_error_func_ref(IntPtr L);

        [DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl)]//[,,m]
        public static extern int load_error_func(IntPtr L, int Ref);

        [DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern int luaopen_i64lib(IntPtr L);//[,,m]

#if (!UNITY_SWITCH && !UNITY_WEBGL) || UNITY_EDITOR
        [DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern int luaopen_socket_core(IntPtr L);//[,,m]
#endif

        [DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern void lua_pushint64(IntPtr L, long n);//[,,m]

        [DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern void lua_pushuint64(IntPtr L, ulong n);//[,,m]

        [DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern bool lua_isint64(IntPtr L, int idx);

        [DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern bool lua_isuint64(IntPtr L, int idx);

        [DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern long lua_toint64(IntPtr L, int idx);

        [DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern ulong lua_touint64(IntPtr L, int idx);

        // push一个lua_CSFunction，n表示要为闭包关联的值的个数
        [DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern void xlua_push_csharp_function(IntPtr L, IntPtr fn, int n);//[-0,+1,m]

        [DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern int xlua_csharp_str_error(IntPtr L, string message);//[-0,+1,m]

        [DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl)]//[-0,+0,m]
        public static extern int xlua_csharp_error(IntPtr L);

        [DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern bool xlua_pack_int8_t(IntPtr buff, int offset, byte field);
        [DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern bool xlua_unpack_int8_t(IntPtr buff, int offset, out byte field);

        [DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern bool xlua_pack_int16_t(IntPtr buff, int offset, short field);
        [DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern bool xlua_unpack_int16_t(IntPtr buff, int offset, out short field);

        [DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern bool xlua_pack_int32_t(IntPtr buff, int offset, int field);
        [DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern bool xlua_unpack_int32_t(IntPtr buff, int offset, out int field);

        [DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern bool xlua_pack_int64_t(IntPtr buff, int offset, long field);
        [DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern bool xlua_unpack_int64_t(IntPtr buff, int offset, out long field);

        [DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern bool xlua_pack_float(IntPtr buff, int offset, float field);
        [DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern bool xlua_unpack_float(IntPtr buff, int offset, out float field);

        [DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern bool xlua_pack_double(IntPtr buff, int offset, double field);
        [DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern bool xlua_unpack_double(IntPtr buff, int offset, out double field);

        [DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr xlua_pushstruct(IntPtr L, uint size, int meta_ref);

        [DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern void xlua_pushcstable(IntPtr L, uint field_count, int meta_ref);

        [DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr lua_touserdata(IntPtr L, int idx);

        // [xlua.c] 获取给定{idx}处值的type_id
        [DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern int xlua_gettypeid(IntPtr L, int idx);

        // 返回lua提供的注册表的有效伪索引
        [DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern int xlua_get_registry_index();

        [DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern int xlua_pgettable_bypath(IntPtr L, int idx, string path);

        [DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern int xlua_psettable_bypath(IntPtr L, int idx, string path);

        //[DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl)]
        //public static extern void xlua_pushbuffer(IntPtr L, byte[] buff);

        //对于Unity，仅浮点组成的struct较多，这几个api用于优化这类struct
        [DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern bool xlua_pack_float2(IntPtr buff, int offset, float f1, float f2);
        [DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern bool xlua_unpack_float2(IntPtr buff, int offset, out float f1, out float f2);
        [DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern bool xlua_pack_float3(IntPtr buff, int offset, float f1, float f2, float f3);
        [DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern bool xlua_unpack_float3(IntPtr buff, int offset, out float f1, out float f2, out float f3);
        [DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern bool xlua_pack_float4(IntPtr buff, int offset, float f1, float f2, float f3, float f4);
        [DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern bool xlua_unpack_float4(IntPtr buff, int offset, out float f1, out float f2, out float f3, out float f4);
        [DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern bool xlua_pack_float5(IntPtr buff, int offset, float f1, float f2, float f3, float f4, float f5);
        [DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern bool xlua_unpack_float5(IntPtr buff, int offset, out float f1, out float f2, out float f3, out float f4, out float f5);
        [DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern bool xlua_pack_float6(IntPtr buff, int offset, float f1, float f2, float f3, float f4, float f5, float f6);
        [DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern bool xlua_unpack_float6(IntPtr buff, int offset, out float f1, out float f2, out float f3, out float f4, out float f5, out float f6);

        [DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern bool xlua_pack_decimal(IntPtr buff, int offset, ref decimal dec);
        [DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern bool xlua_unpack_decimal(IntPtr buff, int offset, out byte scale, out byte sign, out int hi32, out ulong lo64);

        public static bool xlua_is_eq_str(IntPtr L, int index, string str)
        {
            return xlua_is_eq_str(L, index, str, str.Length);
        }

        [DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern bool xlua_is_eq_str(IntPtr L, int index, string str, int str_len);

        [DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr xlua_gl(IntPtr L);

#if GEN_CODE_MINIMIZE
        [DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern void xlua_set_csharp_wrapper_caller(IntPtr wrapper);

        [DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern void xlua_push_csharp_wrapper(IntPtr L, int wrapperID);

        public static void xlua_set_csharp_wrapper_caller(CSharpWrapperCaller wrapper_caller)
        {
#if XLUA_GENERAL || (UNITY_WSA && !UNITY_EDITOR)
            GCHandle.Alloc(wrapper);
#endif
            xlua_set_csharp_wrapper_caller(Marshal.GetFunctionPointerForDelegate(wrapper_caller));
        }
#endif
    }
}
