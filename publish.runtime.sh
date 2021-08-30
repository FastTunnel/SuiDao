#!/bin/bash
## 发布自带运行时的包，用户不需要下载运行时
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
        dotnet publish src/$project/$project.csproj -o=publish/$project.$plate.runtime -c=release -r=$plate -p:PublishSingleFile=true --nologo || exit 1
        # cp src/$project/appsettings.json publish/$project.$plate
        echo
        echo "=========开始打包 ========="
        echo
        cd publish && tar -zcvf $project.$plate.runtime.tar.gz $project.$plate.runtime || exit 1
        cd ../
        # rm -rf publish/$project.$plate
    done
done
