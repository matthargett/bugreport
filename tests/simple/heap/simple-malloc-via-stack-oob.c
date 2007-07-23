#include <stdio.h>
#include <stdlib.h>

int main(int argc, char **argv)
{
	const size_t size = 16;
	char *p = malloc(size);
	p[size] = 0;

	printf(p);

	return 0;
}
