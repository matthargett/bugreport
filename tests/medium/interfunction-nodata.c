#include <stdlib.h>

void bug()
{
	int *array = malloc(1);
	////<bug />
	array[1] = 0;
}

int main(int argc, char **argv)
{
	bug();
}
