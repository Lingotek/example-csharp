Lingotek C# Example:
========================
This code example demonstrates how to send content up to Lingotek and download the resulting translations.

The example has been developed with Mono (.NET opensource implementation), but works with Microsoft .NET.

-----------------------

#### Running with Docker:

Build the Docker container with 

    docker build  -t mono-demo .

And run the example program with:

    docker run -it  -v $PWD/bin:/output mono-demo

-----------------------

See [Lingotek Devzone](http://devzone.lingotek.com/) for documentation and to obtain an access token.

