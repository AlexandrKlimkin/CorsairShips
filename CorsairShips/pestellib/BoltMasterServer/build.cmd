rem Что-то сломалось в публикации в студии, перестала заливать в ажур контейнеры образы
rem Теперь для публикации после паблиша в студии нужно выполнить две команды ниже.
rem Либо запустить этот файл с аргументом командной строки, в котором будет тэг новой версии, например 
rem "build.cmd 105"
rem 
docker tag boltmasterserver:latest gdcompany.azurecr.io:443/boltmasterserver:%1
docker push gdcompany.azurecr.io:443/boltmasterserver:%1