# INI++

An advanced cross-platform INI parser handling abstract sections and inheritance.

## Usage example

**Example.ini**
```ini
; Mammal begins with an underscore, so it's an abstract section which cannot be read and does not show in the sections list, only used for inheritance.
[_MAMMAL]
legs=4

; Cat inherits mammal
[CAT:_MAMMAL]
weight_in_kilograms=4.2

; Kitten inherits cat
[KITTEN:CAT]
weight_in_kilograms=0.8
```

**Example.cs**

```c#
INIFile myINIFile = new INIFile("Example.ini");

Console.WriteLine(myINIFile.ValueExists("cat", "legs"));
Console.WriteLine(myINIFile.ValueExists("cat", "tentacles"));
Console.WriteLine();
Console.WriteLine(myINIFile.GetValue<int>("cat", "legs"));
Console.WriteLine(myINIFile.GetValue<float>("cat", "weight_in_kilograms"));
Console.WriteLine(myINIFile.GetValue<int>("kitten", "legs"));
Console.WriteLine(myINIFile.GetValue<float>("kitten", "weight_in_kilograms"));
Console.WriteLine();
myINIFile.SetValue<float>("cat", "weight_in_kilograms", 8.4); // How did the cat get so fat?
Console.WriteLine(myINIFile.GetValue<float>("cat", "weight_in_kilograms"));
```
**Output**

```
true
false

4
4.2
4
0.8

8.4
```