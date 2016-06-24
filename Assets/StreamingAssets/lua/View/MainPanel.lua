local transform;
local gameObject;

MainPanel = {};
local this = MainPanel;

--启动事件--
function MainPanel.Awake(obj)
	gameObject = obj
	transform = obj.transform

	this.InitPanel()
	logWarn("Awake lua--->>"..gameObject.name)
end

--初始化面板--
function MainPanel.InitPanel()
--    this._btnSend = transform.Get
end

--单击事件--
function MainPanel.OnDestroy()
	logWarn("OnDestroy---->>>")
end