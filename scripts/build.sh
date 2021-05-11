#!/usr/bin/env bash

git checkout -- . ; git pull && source .external-config && sudo docker build -t malninstallconfiguration ../src/ && sudo docker-compose down && sudo docker-compose up -d
