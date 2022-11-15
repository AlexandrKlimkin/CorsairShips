pushd pestellib
Tools\git\cmd\git.exe add .
Tools\git\cmd\git.exe stash
popd

pestellib\Tools\git\cmd\git.exe submodule foreach git reset --hard
pestellib\Tools\git\cmd\git.exe submodule foreach git clean -d -f
pestellib\Tools\git\cmd\git.exe submodule update --checkout --recursive 
pause