#include <stdio.h>
#include <stdlib.h>

int main(int argc, char **argv)
{
	char *p = malloc(16);
	p[-1] = 0;

	printf(p);

	return 0;
}
