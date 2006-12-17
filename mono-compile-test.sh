gmcs -debug -t:exe -out:bin/Debug/bugreport.exe -r:lib/nunit/nunit.framework.dll -r:lib/nunit/nunit.mocks.dll *.cs && mono --debug lib/nunit/nunit-console.exe bin/Debug/bugreport.exe
