# 概述
TIA Portal Openness是西门子提供的一个用于工程组态工作流自动化的API，它允许开发者通过编程与TIA Portal软件进行交互，实现自动化和个性化的工程组态。通过TIA Portal Openness，开发者可以更加灵活地利用TIA Portal的功能，为自动化工程提供更加高效和定制化的解决方案。<br>
我使用TIA Portal Openness开发了一个自动生成Modbus程序的软件，通过输入简单的参数配置如波特率、校验、从站站号、读/写、地址、长度、数据类型、名称，即可自动生成一个包含了轮询和变量表的完整Modbus程序。
# 开始
点开项目<TIA程序生成>中的引用，查看Siemens.Engineering和Siemens.Engineering.Hmi是否有异常，如果不存在则鼠标右键添加应用，点击浏览查找路径：C:\Program Files\Siemens\Automation\Portal V*\PublicAPI\V*\Siemens.Engineering.dll和C:\Program Files\Siemens\Automation\Portal V*\PublicAPI\V*\Siemens.Engineering.Hmi.dll，引用Siemens.Engineering.dll和Siemens.Engineering.Hmi.dll，然后清理解决方案-重新生成解决方案。
# 支持的TIA Portal版本
TIA Portal V14 SP1<br>
TIA Portal V15<br>
TIA Portal V15 SP1<br>
TIA Portal V16<br>
TIA Portal V17 (已测试）<br>
TIA Portal V18 (已测试）<br>
TIA Portal V19 (已测试 by胡佳）<br>
# 支持的框架
.NET Framework 4.8.1
# 编译
Visual Studio 2022（可以免费下载 Community Edition）
# 不支持的系统版本
家庭版操作系统
# 运行测试
打开带项目的TIA Portal程序，PLC中组态通信模块（目前尚不支持CB信号板和ET200的CM模块），以管理员身份运行Visual Studio 2022,启动程序之后软件会检测当前登录账户是否在Siemens TIA Openness用户组，如果不在Siemens TIA Openness用户软件会弹窗提示，用户可选择由软件添加用户组还是自行添加用户组，添加完成后重启计算机。
# 致谢
luis.SPS.standard   https://www.youtube.com/@luis.sps.standard7444 <br>
胡佳（测试TIA Portal V19） QQ:2859299484 <br>
