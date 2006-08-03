#include <stdlib.h>

void bug()
{
	int *array = malloc(0);
	////<bug />
	array[0] = 0;
}

int main(int argc, char **argv)
{
	bug();
}
