#include <stdlib.h>

void bug(size_t size)
{
	int *array = malloc(size);
	array[0] = 0;
}

int main(int argc, char **argv)
{
	////<bug />
	bug(0);
}
