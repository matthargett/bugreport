#include <stdlib.h>
#include <string.h>
#include <stdio.h>

int main (int argc, char**argv)
{
	char name[16] = {0};
	int i = 0;

	if (1 == argc)
		exit(1);

	while (argv[1][i] != '\\')
	{
		name[i] = argv[1][i];
		i++;
	}

	printf(name);
}
