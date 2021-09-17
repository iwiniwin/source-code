--[[--
注册CS一个类到Lua中后，Lua注册表的模拟结构
比如在Lua中访问CS.A.B.C
userdata : xlua_pushcsobj产生的userdata，cs对象在lua中的代理，它的元表是类型表
refi : 通过luaL_ref将一个表添加到注册表后返回的索引
@module lua_registry
@author iwiniwin

Date   2021-09-03 19:30:39
Last Modified by   iwiniwin
Last Modified time 2021-09-03 19:30:39
]]
local lua_registry = {
    ["A.B.C"] = {
        [1] = type_id2,
        __name = "A.B.C",
        xlua_tag = 1,
        __gc = "csfunc:StaticLuaCallbacks.LuaGC",
        __tostring = "csfunc:StaticLuaCallbacks.ToString",
        __index = function(obj, key)
            -- gen_obj_indexer生成的闭包函数
            -- 查找 存放非静态域的那些表，以及通过父类的__index查找
        end,
        __newindex = function(obj, key, value)
            -- gen_obj_newindexer生成的闭包函数
            -- 设置 存放非静态域的那些表，以及通过父类的__newindex设置
        end
    },
    ["System.RuntimeType"] = {
        [1] = type_id1,
    },
    LuaIndexs = {
        ["userdata:typeof(A.B.C)_obj"] = lua_registry["A.B.C"].__index
    },
    LuaNewIndexs = {
        ["userdata:typeof(A.B.C)_obj"] = lua_registry["A.B.C"].__newindex
    },
    xlua_csharp_namespace = {  -- 就是CS全局表
        A = {
            B = {
                C = {  -- cls_table 其元表是meta_table
                    UnderlyingSystemType = "userdata:typeof(A.B.C)_obj",
                    ["静态只读字段1"] = "value",
                    ["静态只读字段2"] = "value",
                    ["静态方法1"] = "func",
                },
            }
        },
        ["userdata:typeof(A.B.C)_obj"] = lua_registry[xlua_csharp_namespace][A][B][C],  -- cls_table
    },
    LuaClassIndexs = {
        ["userdata:typeof(A.B.C)_obj"] = meta_table.__index
    },
    LuaClassNewIndexs = {
        ["userdata:typeof(A.B.C)_obj"] = meta_table.__newindex
    },
    
    ["refi:cache_ref"] = {  -- 缓存表
        ["index:csobj_in_translator_objects"] = "userdata:typeof(A.B.C)_obj",  -- 其元表是lua_registry["System.RuntimeType"]
    },
    ["refi:type_id1"] = lua_registry["System.RuntimeType"],  -- type_id = meta_ref
    ["refi:type_id2"] = lua_registry["A.B.C"],  -- type_id = meta_ref
}

meta_table = {
    __call = "csfunc:类的构造函数",
    __index = function(obj, key)
        -- gen_cls_indexer生成的__index闭包函数
        -- 查找 存放静态域的那些表，以及通过父类的__index查找
    end,
    __newindex = function(obj, key, value)
        -- gen_cls_newindexer生成的__newindex闭包函数
        -- 设置 存放静态域的那些表，以及通过父类的__newindex设置
    end
}
