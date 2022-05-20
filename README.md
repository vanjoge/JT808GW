 * [JT808GW](#JT808GW)

# JT808GW
本网关当前并非全功能网关，主要实现与RTVS对接演示。

当前仅实现了常用协议层面接入和网关公共逻辑(如：指令、分包、校验等)，所有消息收到后均只应答协议内容，不做与RTVS无关的逻辑处理，且并未保存数据，如需用于生产，请自行实现。

实现了sip客户端，支持将JT808(部标)转GB28181(国标)信令发送给其他国标平台，并通过RTVS支持了JT1078转GB28181视频流，可在其他国标平台直接观看设备的实时视频。

TODO?
> 加入保存数据逻辑，将最近的数据存入redis，历史数据写本地文件
> 
> 加入转储服务，实现文件到数据库、kafka等写入
> 
> 加入简单UI，支持车辆位置监控、历史轨迹回放等基础功能
> 
> 修改解析库等基础库，进一步提高性能


## 关联项目地址

RTVS

[https://github.com/vanjoge/RTVS](https://github.com/vanjoge/RTVS)

[https://gitee.com/vanjoge/RTVS](https://gitee.com/vanjoge/RTVS)

gbSip(国标28181接入网关 实现SIP信令并对接RTVS接口)

[https://github.com/vanjoge/gbSip](https://github.com/vanjoge/gbSip)

[https://gitee.com/vanjoge/gbSip](https://gitee.com/vanjoge/gbSip)


QQ交流群：614308923