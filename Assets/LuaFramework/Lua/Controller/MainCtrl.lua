--region *.lua
--Date
--此文件由[BabeLua]插件自动生成

require "Common/define"
require "3rd/pbc/protobuf"

MainCtrl = {};
local this = MainCtrl;
--构建函数--
function MainCtrl.New()
	logWarn("MainCtrl.New--->>");
	return this;
end

function MainCtrl.Awake()
	logWarn("MainCtrl.Awake--->>");
	panelMgr:CreatePanel('Main', this.OnCreate);
--    networkMgr:AddEvent(999999,function() log("message is call back!") end,nil);
end

--启动事件--
function MainCtrl.OnCreate(obj)
    logWarn("进入MainCtrl的OnCreate方法");
	this.gameObject = obj;
	this.transform = obj.transform;
	this.panel = this.transform:GetComponent('UIPanel');
	this.prompt = this.transform:GetComponent('LuaBehaviour');
    this.btnSend = find("BG/btn_send");
    logWarn("Start lua--->>"..this.gameObject.name);
    this.prompt:AddClick(this.btnSend,function (go) 
    local data = getMo({"123","321"});
    networkMgr:sendMessage(999999,function (sessionData) log("message is call back!") end,nil,data);
    end);
--	resMgr:LoadPrefab('prompt', { 'PromptItem' }, this.InitPanel);
end

--初始化面板--
function MainCtrl.InitPanel(objs)
--	local count = 100; 
--	local parent = PromptPanel.gridParent;
--	for i = 1, count do
--		local go = newObject(objs[0]);
--		go.name = 'Item'..tostring(i);
--		go.transform:SetParent(parent);
--		go.transform.localScale = Vector3.one;
--		go.transform.localPosition = Vector3.zero;
--        prompt:AddClick(go, this.OnItemClick);

--	    local label = go.transform:FindChild('Text');
--	    label:GetComponent('Text').text = tostring(i);
--	end
end
--关闭事件--
function MainCtrl.Close()
	panelMgr:ClosePanel(CtrlNames.Main);
end
--endregion
