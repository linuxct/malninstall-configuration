#!/usr/bin/env bash

mkdir -p logs && mkdir -p /usr/share/man/man1 && mkdir -p /home/malninstall/files && mkdir -p /home/malninstall/files/Downloads
apt update && apt install -y wget unzip openjdk-11-jdk-headless
wget -O /home/malninstall/files/apktool.jar https://bitbucket.org/iBotPeaches/apktool/downloads/apktool_2.5.0.jar
wget -O /home/malninstall/files/asbt.zip https://dl.google.com/android/repository/build-tools_r30.0.1-linux.zip
unzip /home/malninstall/files/asbt.zip -d /home/malninstall/files/
ln -s /home/malninstall/files/android-11/apksigner /home/malninstall/files/apksigner
ln -s /home/malninstall/files/android-11/zipalign /home/malninstall/files/zipalign
wget -O /home/malninstall/files/framework.apk https://dumps.tadiphone.dev/dumps/google/blueline/-/raw/blueline-user-11-RQ2A.210405.006-7214111-release-keys/system/system/framework/framework-res.apk
java -jar /home/malninstall/files/apktool.jar if /home/malninstall/files/framework.apk
wget -O /home/malninstall/files/testkey.pk8 https://github.com/aosp-mirror/platform_build/raw/android11-platform-release/target/product/security/testkey.pk8
wget -O /home/malninstall/files/testkey.x509.pem https://github.com/aosp-mirror/platform_build/raw/android11-platform-release/target/product/security/testkey.x509.pem
wget -O /home/malninstall/files/template.zip https://github.com/linuxct/malninstall-template/releases/download/20210510/smali-20210510.zip