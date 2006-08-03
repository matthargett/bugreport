#include <stdlib.h>

char getTaint(char **argv)
{
	return **argv;
}

void bug(char t)
{
	int *array = malloc(0);
	////<exploitable />
	array[0] = t;
}

int main(int argc, char **argv)
{
	char t = getTaint(argv);
	bug(t);
}
