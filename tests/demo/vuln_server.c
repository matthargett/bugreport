/* 

-----bugreport supplemental note: ---------------------------------------------

The following code is from "Secure Programming Educational Material",
accessable at http://projects.cerias.purdue.edu/secprog/ (accessed on
2006-09-01 17:24:05 UTC). This code is directly accessable at
http://projects.cerias.purdue.edu/secprog/class2/vuln_server.c (accessed on
2006-09-01 17:24:58 UTC).

While there is no license or copyright information in vuln_server.c itself, or
in the directory it was found in, below is a copy of the terms found at
http://projects.cerias.purdue.edu/secprog/ (accessed on 2006-09-01 17:24:05
UTC) which govern the way in which vuln_server.c may be used.

=//=

Pascal Meunier, Ph.D., M.Sc., CISSP
Purdue University CERIAS

Terms

You are free to copy, distribute, display, and perform the work; and to make
derivative works, under the following conditions.

    * You must give the original author and other contributors credit.
    * The work will be used for personal or non-commercial educational uses
    only, and not for commercial activities and purposes.
    * For any reuse or distribution, you must make clear to others the terms of
    use for this work.
    * Derivative works must retain and be subject to the same conditions, and
    contain a note identifying the new contributor(s) and date of modification.
    * For other uses please contact the Purdue Office of Technology
    Commercialization. 

Copyright (2004) Purdue Research Foundation. All rights reserved.

Developed thanks to the support of Symantec Corporation, NSF SFS Capacity
Building Program (Award Number 0113725) and the Purdue e-Enterprise Center

Contributors:
Jennifer Richardson, Jared Robinson, Alan Krassowski, Craig Ozancin, Tim Brown,
Wes Higaki, Melissa Dark, Chris Clifton, Gustavo Rodriguez-Rivera.

Thanks to Michael Howard for reviewing several sets of slides!

CERIAS, Purdue University
Recitation Building
656 Oval Drive
West Lafayette IN 47907-2039

phone (800)494-4419
fax (765)496-3181

=//=

-----end note------------------------------------------------------------------

   Description: Simple server program with intentional vulnerabilities for
   learning purposes.
  
   WARNING: Do not use this code as an example to write your own production
   server application. This is toy code, and is not robust. It WILL allow
   attackers to gain access to your machine.
  
   LICENSE: BSD-style (without the advertising clause)

   To Compile under Windows:
     cl vuln_server.c /link ws2_32.lib

   To Compile under Linux or Cygwin:
     make vuln_server

   To Compile under Sun:
     gcc -lsocket -lnsl vuln_server

   Running:
     From one command prompt, type "vuln_server 5700"
     From another prompt, type "telnet 127.0.0.1 5700"

   Buffer overflow exercises:
   1. What happens when you type in the string "Hello world!"?
      (You'll need to apply this knowledge in the format string exercises)
   2. Type in a long string (more than 100 characters). 
      It should crash. Where is the buffer overflow?
   3. Fix the buffer overflow, recompile, and demonstrate that it doesn't  
      crash on long input lines any more.
   4. Bonus: Can you get a shell?
   
   Format string exercises:
   1. Where is the format string problem?
   2. How do you crash the program? Hint: use %s
   3. How do you print the contents of memory to divulge the secret which  
      is 0xdeadc0de? Hint: use %08x
   4. Bonus: Can you change the contents of "secret" without crashing the
      program?
   5. Bonus: Can you get a shell?
 */

#include <stdio.h>
#include <stdlib.h>
#include <string.h>
#include <stdarg.h>

#ifdef _WIN32
#  include <winsock2.h>
#else
#  include <sys/types.h>
#  include <sys/socket.h>
#  include <netinet/in.h>
#  include <unistd.h>
#  define SOCKET int
#endif



#define END_LINE '\n'

void logit(FILE *fp, char *client, char *message);
int read_line(int fd, char *buf, int buf_size);
void reverse(char *dest, const char *source);
int write_client(int fd, const char *fmt, ...);
void winsock_init();

int main(int argc, char *argv[]) {
    SOCKET listenfd, sockfd;
    int clientLen, serverPort;
    char reversed_line[100];
    struct sockaddr_in clientAddress, serverAddress;
    char line[500];
    FILE *fpLog = NULL;
    int secret = 0xDEADC0DE;
    int clientQuit = 0;

    if (argc != 2)
    {
        printf("Usage: %s <port>\n", argv[0]);
        return 1;
    }
#ifdef _WIN32
    winsock_init();
#endif

    serverPort = atoi(argv[1]);
    /* create socket */
    listenfd = socket(PF_INET, SOCK_STREAM, 0);
    if ( listenfd < 0 )
    {
        perror("socket error ");
        return 1;
    }

    /* bind server port */
    memset(&serverAddress, 0, sizeof(serverAddress));
    serverAddress.sin_family = AF_INET;
    // serverAddress.sin_addr.s_addr = htonl(INADDR_ANY);
    serverAddress.sin_addr.s_addr = inet_addr("127.0.0.1");
    serverAddress.sin_port = htons(serverPort);

    if ( -1 == bind(listenfd, (struct sockaddr *) &serverAddress, sizeof(serverAddress)) )
    {
        perror("bind error");
        return 1;
    }

    if (-1 == listen(listenfd, 5))
    {
        perror("listen error");
        return 1;
    }

    fpLog = fopen("server.log", "w");
    if (fpLog == NULL)
    {
        perror("couldn't open server.log for writing");
        return 1;
    }

    while ( ! clientQuit )
    {
        printf("%s: waiting for connection on TCP port %u\n\n", argv[0], serverPort);

        clientLen = sizeof(clientAddress);
        sockfd = accept(listenfd, (struct sockaddr *) &clientAddress, &clientLen);
        if ( -1 == sockfd )
        {
            perror("error accepting connection ");
            return 1;
        }

        write_client(sockfd, "Type QUIT on a line by itself to quit\n");

        /* init line */
        memset(line, 0, sizeof(line));

        clientQuit = 0;
        while ( !clientQuit &&  read_line(sockfd, line, sizeof(line)) != 0)
        {
            printf( "%s:%d %s\n", 
                    inet_ntoa(clientAddress.sin_addr),
                    ntohs(clientAddress.sin_port), 
                    line);
            if (0 == strncmp(line, "QUIT", 4))
            {
                clientQuit = 1;
                write_client(sockfd, "Goodbye\n");
                close(sockfd);
            }
            else
            {
                reverse(reversed_line, line);
                write_client(sockfd, reversed_line);
                write_client(sockfd, "\n");
                // logit(fpLog, inet_ntoa(clientAddress.sin_addr), line);
            }
            memset(line, 0, sizeof(line));
        }
    }
#ifdef _WIN32
    WSACleanup();
#endif
}

int write_client(int fd, const char *fmt, ...)
{
    va_list args;
    char message[100];

    if (fmt == NULL)
        return 0;
    va_start(args, fmt);
#ifdef _WIN32
    vsprintf(message, fmt, args);
#else
    vsnprintf(message, sizeof(message), fmt, args);
#endif
    va_end(args);
    return send(fd, message, strlen(message), 0);
}

int read_line(int fd, char *buf, int buf_size)
{
    int nleft, nread, buf_len;

    nleft = buf_size;
    while (nleft > 0)
    {
        if ((nread = recv(fd, buf, nleft, 0)) < 0)
        {
            return nread;
        }
        else if (nread == 0)
        {
            break;
        }
        nleft -= nread;
        buf += nread;
        if (*(buf - 1) == END_LINE)
        {
            break;
        }
    }
    buf_len = buf_size - nleft;
    if (buf_len > 0)
    {
        *(buf - 1) = '\0';
    }

    return buf_len;
}

void logit(FILE *fp, char *client, char *message)
{
    fprintf(fp, "%s", client);
    fprintf(fp, message);
    fprintf(fp, "\n");
    fflush(fp);
}

void reverse(char *dest, const char *source)
{
    int len, i;
    len = strlen(source);
    i = 0;
    while (len > 0)
    {
        dest[i++] = source[--len];
    }
    dest[i] = '\0';
}

#ifdef _WIN32
void winsock_init()
{
    static WSADATA wsaData;    
    int wsaret;

    if (WSAStartup(MAKEWORD(2,2), &wsaData) != NO_ERROR)
    {
        fprintf(stderr, "Couldn't start winsock\n");
        WSACleanup();
        exit(1);
    }
}
#endif
