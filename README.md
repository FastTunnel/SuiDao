## SuiDao.io
基于[FastTunnel](https://github.com/SpringHgui/FastTunnel)二次开发的内网穿透服务

***
官网: [https://suidao.io](https://suidao.io)

***
## 免费使用步骤

- 登录官网[https://suidao.io](https://suidao.io)注册账号
- 创建一个免费的隧道，同时在个人中心获取 `accesskey` 密钥，此密钥用于客户端登录。
- 在 [Releases](https://github.com/SpringHgui/FastTunnel.SuiDao/releases)页面，下载您的系统所对应的客户端。
- 运行SuiDao客户端，输入accesskey即可登录完成，开始享受你的内网穿透之旅吧。

## SuiDao.Client设置自启&自动登录

各个操作系统如何实习开机自启某个程序不做介绍  
启动程序时传参如下，可实现程序启动时自动使用执行的accesskey进行登录  

`SuiDao.Client login accesskey`  

accesskey替换你自己的key

## 我有公网服务器，想要搭建自己的专属穿透服务

本项目是FastTunnel基于开发，和[suidao.vue](https://github.com/SpringHgui/suidao.vue)等多个服务组成（其他不分尚未开源），不能直接用于自己搭建服务使用。
移步至 [FastTunnel](https://github.com/SpringHgui/FastTunnel) 使用FastTunnel项目直接搭建。

## 源码

前端Web源码：[suidao.vue](https://github.com/SpringHgui/suidao.vue)  
业务api源码：尚未开放 

## Welcome PR
