# PioSolverConnection
Examples on how to communicate with PioSolver in different languages

The example code shows how to start a solver process and how to send commands and parse responses from PioSolver. 
The documentation of the PioSolver interface can be found here:
https://piofiles.com/docs/upi_documentation/

# Python
 
### Requirements

* Install Python version at least 3.6.
* Python example can be found in python_2_0 directory. It consists of a SolverConnection/solver.py with code that allows the user connect to solver and file `runme.py` which contains some examples of running commands.

### Running the code

Example file connects to the solver and requests some data from a specified save file.

1. Open PowerShell or Command Prompt (Windows + R, then type "cmd" and click "OK") 
1. Navigate to a directory with unpacked python client 
1. Try running command: ` python runme.py <path_to_solver> <path_to_tree> `

(Example: ` python runme.py d:\PioSOLVER\PioSOLVER2-edge.exe d:\Piosolver\saves\ExampleCOsmall\Qs7s3h.cfr) `

If everything works correctly, it should print out output similiar to the following:

```
Solver connected successfully
r
ROOT
Ks 9s 4h
0 0 180
1 children
flags: INCOMPLETE_TREE ISOMORPHISM_ON PIO_CFR

910
1 1 1 1 1 1 0 0 0 (...)
0 0 0 0 0 0 0.45 (...)
#Type#NoLimit
#Range0#AA,KK,(...)
#Range1#AA:0.25,(..)
#Board#4s 7h 8c
(... tree config)
show_metadata ok!
load_tree ok!
2d2c 2h2c 2h2d 2s2c (... hands order)
Range OOP:
0 0 0 0 0 0 0.45 (...)
Range IP:
1 1 1 1 1 1 0 0 0 (...)
EV IP:
inf inf inf inf inf inf 8.83594799 (...)
0 0 0 0 0 0 171.224976 0 0 0 0 (...)
Closing connection:
Connection closed.
Done.
```

# C# 

### Requirements
 * Download Visual Studio
 * C# example project can be found in C_sharp_2_0 directory. It contains code to connect with Solver (folder Connection), some simple plugins which serve as examples on how to use SolverConnection (FileRebuilder, FileReporter, FileShrinker, RangesPrinter) and Program.cs, which parses command line arguments and runs one of the plugins.

### Running C# client 

 * Open the solution file `Client2.sln` in Visual Studio 
 * Build the project 
 * run the project with example arguments. (you can either run the Client2.exe from command line or directly from VS (google 'command-line parameters in Visual Studio')
 
Example command line arguments for the example program are:

* `print_ranges -solver <path_to_solver> -tree <path_to_tree>`
* `files_info -solver <path_to_solver> -directory <directory_with_trees>`
* `shrink_saves -solver <path_to_solver> -savesdirectory <directory_with_saves> -outputdirectory <empty_directory> -size <desired_size>`
* `rebuild_file -solver <path_to_solver> -tree <path_to_tree>`

If everything works correctly, it should print out output similiar to the following:

```
> Client2.exe print_ranges -solver D:\PioSOLVER-2\PioSOLVER2-edge.exe -tree Ks9s4h.cfr
```

```
load_tree ok!
OOP range:
0 0 0 0 0 0 0.45 0 0 (...)
IP range:
1 1 1 1 1 1 0 0 0 0 (...)
```

### More example C# code 

In directory `Plugins` of the example C# client there are few examples on how to achieve certain tasks.

* print_ranges (prints ranges to console)
Arguments: -solver <path_to_solver> -tree <path_to_tree>

* files_info (shows some basic info on files and prints it to console)
Arguments: -solver <path_to_solver> -directory <directory_with_trees>

* shrink_saves (loads all trees from directory and saves smaller version in output directory). Options for desired_size: to_no_turns, or to_no_rivers
Arguments: -solver <path_to_solver> -savesdirectory <directory_with_saves> -outputdirectory <empty_directory> -size <desired_size>

* rebuild_file (loads a tree, rebuilds forgotten paths in tree and saves it again)
Arguments: -solver <path_to_solver> -tree <path_to_tree>




