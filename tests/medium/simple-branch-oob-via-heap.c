#include <stdlib.h>

int main(int argc, char **argv)
{
	size_t size = 15;
	int count = 0;

	if ('.' <= **argv)
		count -= 2;

	if ('\\' != **argv)
		count += 260;

	void *p = malloc(size + count);

	////<exploitable />
	((char *)p)[size] = 0;
}
