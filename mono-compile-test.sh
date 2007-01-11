mkdir -p bin/
mkdir -p bin/Debug/
gmcs -debug -t:exe -out:bin/Debug/bugreport.exe -r:lib/nunit/nunit.framework.dll -r:lib/nunit/nunit.mocks.dll *.cs && cp lib/nunit/nunit.framework.dll lib/nunit/nunit.mocks.dll bin/Debug && mono --debug lib/nunit/nunit-console.exe bin/Debug/bugreport.exe && mono lib/gendarme/gendarme.exe bin/Debug/bugreport.exe
