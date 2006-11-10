#include <stdlib.h>

void bug(int *array)
{
	////<bug />
	array[4] = 0;
}

int main(int argc, char **argv)
{
	int *array = malloc(4);
	bug(array);
}
