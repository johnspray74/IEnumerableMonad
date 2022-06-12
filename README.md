<details open="open">
  <summary>Table of Contents</summary>
  <ol>
    <li><a href="#about-the-project">About the project</a></li>
    <li><a href="#To-run-the-example-application">To run the example application</a></li>
    <li><a href="#Built-with">Built with</a></li>
    <li><a href="#contributing">Contributing</a></li>
    <ul>
        <li><a href="#Future-work">Future work</a></li>
    </ul>
    <li><a href="#background">Background</a></li>
    <li><a href="#Authors">Authors</a></li>
    <li><a href="#license">License</a></li>
  </ol>
</details>


# About the project

Demo code of a List<T> and IEnumerable monad and equivalent ALA functionality in C#. Code used for online book at abstractionlayeredarchitecture.com 

The purpose of this project is example code to compare monads and ALA ([Abstraction Layered Architecture](AbstractionLayeredArchitecture.md)).

It's not intended as a useful implementation of this monad, which already exists in SelectMany anyway.
It's to show how monads work with actual code, and compare that with the actual code of ALA.
It's also to compare how monads compose functions, and ALA composes with more versatile objects.
This versatility allows us to create a domain abstraction class that allows us to compose functions the same as those used by monads.
We also provide a Bind function in the ALA version to make the application code look exactly like the monad version.
We wouldn't normally do that in ALA, but it shows that we can get the monad syntax exactly as well.

Monads are a two-layer pattern. The monad implementaion is in the Monad folder, and the application is in the Application folder.
ALA is a 4 layer pattern so the implementation is in the Domain Abstractions, Programming Paradigms and Foundation folders, and the application is again in the Application folder.

This example is one of a set of examples implementing different types of monads. Others are Continuation, IObservable, IMaybe and State monads.

In this example application, the composed functions take the inpu and return three numbers with 1,2,3 append the end of the input number.

If you don't understand monads, the section in chapter 3 of the online book fully explains them (I think in a better way than other explanations).
It wasn't until I implemented them myself with this set of examples (for the purpose of comparing what they do and how they work with ALA) that
I realised that all the previous explanations I had read had been inadequate and even inaccurate. 
For example, some high level explanations suggest that the monad object returned by the function is like a value in a box in a box,
which simply needs unwraping to make the type correct, and that is what is returned by the bind function. 
None of these monads do that. The returned value of the function is never a wrapped wrapped value.
Rather the monad object returned by the function has the same type as the Bind's return value.
But it is not the object that Bind returns. Bind generally creates a new monad object and uses the one returned by the function to get data for the new object in some way that makes sense for the particular monad.
So go onto the ALA  web site <http://www.abstractionlayeredarchitecture.com> and have a read if you really want to understand monads. 
  
## To run the demo monad application

1. Clone this repository or download as a zip.
2. Open the solution in Visual Studio 2019 or later
3. When the application runs, you will see the program outputs 111, 112, 113, 121 etc.


## Built with

C#, Visual Studio 2019


## Contributing

Contributions are what make the open source community such an amazing place to be learn, inspire, and create. Any contributions you make are **greatly appreciated**.

1. Fork the project using the button at top right of the main Github page or (<https://github.com/johnspray74/ALAExample/fork>)
2. Create your feature branch (`git checkout -b feature/AmazingFeature`)
3. Commit your changes (`git commit -am 'Add AmazingFeature'`)
4. Push to the branch (`git push origin feature/AmazingFeature`)
5. Open a Pull Request


### Future work

If someone who is expert with C# would like to look over my code and improve it that would be appreciated.
If someone who is expert with Haskel, closure, Swift, Java, Python or Rust would like to contribute a version of this example code that would be great.

## Background


## Authors

John Spray

### Contact

John Spray - johnspray274@gmail.com



## License

This project is licensed under the terms of the MIT license. See [License.txt](License.txt)

[![GitHub license](https://img.shields.io/github/license/johnspray74/ALAExample)](https://github.com/johnspray74/ALAExample/blob/master/License.txt)

## Acknowledgments


