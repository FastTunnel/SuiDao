#!/bin/bash
## 不含运行时，可大大减少软件包的大小，需要运行的平台安装对应的.net运行时环境
rm -rf publish/*
projects=("SuiDao.Client") # "SuiDao.Server"
plates=("win-x64" "win-x86" "win-arm" "osx-x64" "linux-arm" "linux-x64")
for project in ${projects[*]}; do
    echo
    echo "=========开始发布：${project} ========="
    echo
    for plate in ${plates[*]}; do
        echo "plate=${plate}"
        echo src/$project/$project.csproj
        dotnet publish src/$project/$project.csproj -o=publish/$project.$plate -c=release --nologo || exit 1
        # cp src/$project/appsettings.json publish/$project.$plate
        echo
        echo "=========开始打包 ========="
        echo
        cd publish && tar -zcvf $project.$plate.tar.gz $project.$plate || exit 1
        cd ../
        # rm -rf publish/$project.$plate
    done
done
