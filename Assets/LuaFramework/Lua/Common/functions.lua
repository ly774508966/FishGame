
--输出日志--
function log(str)
    Util.Log(str);
end

--错误日志--
function logError(str) 
	Util.LogError(str);
end

--警告日志--
function logWarn(str) 
	Util.LogWarning(str);
end

--查找对象--
function find(str)
	return GameObject.Find(str);
end

function destroy(obj)
	GameObject.Destroy(obj);
end

function newObject(prefab)
	return GameObject.Instantiate(prefab);
end

--创建面板--
function createPanel(name)
	PanelManager:CreatePanel(name);
end

function child(str)
	return transform:FindChild(str);
end

function subGet(childNode, typeName)		
	return child(childNode):GetComponent(typeName);
end

function findPanel(str) 
	local obj = find(str);
	if obj == nil then
		error(str.." is null");
		return nil;
	end
	return obj:GetComponent("BaseLua");
end

require "3rd/pbc/protobuf"
function sendMessage(pName,data)
    local code = protobuf.encode("MoData", data)
    ----------------------------------------------------------------
    local buffer = ByteBuffer.New();
    buffer:WriteBuffer(code);
    networkMgr:SendMessage(buffer);
end

function getMo(val)
    local temp = "";
    if val ~= nil and #val >=1 then 
        for k,v in ipairs(val) do  
            temp = temp..v.."Ω";
        end
        temp = string.sub(temp,0,string.len(temp)-1);
    end
    local mo = {
        msg = "";
    }
    mo.msg = temp;
    log("获得Mo数据"..mo.msg);
    local data = protobuf.encode("com.ftkj.proto.MoData",mo);
    return data;
end
