#include <stdlib.h>

int main(int argc, char **argv)
{
	size_t size = 15;
	int count = 0;

	if (0 == argc)
		count++;

	if (0 != argc)
		count--;

	void *p = malloc(size + count);

	((char *)p)[size] = 0;
}
