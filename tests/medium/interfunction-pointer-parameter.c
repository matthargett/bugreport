#include <stdlib.h>

void bug(int *array)
{
	////<bug />
	array[0] = 0;
}

int main(int argc, char **argv)
{
	int *array = malloc(0);
	bug(array);
}
