@echo off
color 0e
@echo ==================================
@echo 提醒：请右键本文件，用管理员方式打开。
@echo ==================================
@echo Start Install SuiDao.Client

sc create SuiDao.Client binPath=%~dp0\SuiDao.Client.exe start= auto 
sc description SuiDao.Client "FastTunnel-开源内网穿透服务，仓库地址：https://github.com/SpringHgui/FastTunnel"
Net Start SuiDao.Client
pause