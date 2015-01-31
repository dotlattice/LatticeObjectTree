# LatticeObjectTree [![Build status](https://ci.appveyor.com/api/projects/status/uptdctnkaitlyslg)](https://ci.appveyor.com/project/dotlattice/latticeobjecttree)

LatticeObjectTree is a .NET library for working with a tree of objects.  This allows for all kinds of applications, including comparing objects recursively and generating code from an object.

## Installation

There are several ways to install this library:

* Install the [NuGet package](https://www.nuget.org/packages/LatticeObjectTree/)
* Download the assembly from the [latest release](https://github.com/dotlattice/LatticeObjectTree/releases/latest) and install it manually
* Copy the parts you want into your own project

This entire library is released into the [public domain](https://github.com/dotlattice/LatticeObjectTree/blob/master/LICENSE).  So you can copy anything from this library into your own project without having to worry about attribution or any of that stuff.

## Use

The easiest way to create an object tree is to use the static ObjectTree.Create methods:

```c#
var tree = ObjectTree.Create("hello world");
```

A tree is basically just a collection of nodes, and nodes are connected to each other through edges.

So for example, say you have this Person class:

```c#
public class Person
{
    public string Name { get; set; }
    public Person BestFriend { get; set; }
}
```

And let's say we create a tree for a Person like this:

```c#
var apollo = new Person
{
    Name = "Apollo",
    BestFriend =  new Person 
    { 
        Name = "Robert",
        BestFriend = new Person
        {
            Name = "Clubber"
        },
    },
};
var apolloTree = ObjectTree.Create(apollo);
```

Here's a visualization of the tree for this person:

<pre>
         +---------------+                             
         |Person (Apollo)|                             
         +-----+---------+                             
               |                                       
       Name    |   BestFriend                          
     +---------+----------+                            
     |                    |                            
+----+---+        +-------+-------+                    
|"Apollo"|        |Person (Robert)|                    
+--------+        +-------+-------+                    
                          |                            
                  Name    |   BestFriend               
                +---------+----------+                 
                |                    |                 
            +---+----+       +-------+--------+        
            |"Robert"|       |Person (Clubber)|        
            +--------+       +-------+--------+        
                                     |                 
                             Name    |   BestFriend    
                           +---------+----------+      
                           |                    |      
                      +----+----+                      
                      |"Clubber"|              NULL    
                      +---------+                      
</pre>

There are two main parts to an object tree:

* Nodes - the values.  In this example, there is a node for each Person object and for each "Name" string.
* Paths - the connections between nodes.  In this example, all of the paths are properties (the "Name" property and the "BestFriend" property.

So to use the tree, you can iterate through these nodes and the paths that connect them:

```c#
var nodeQueue = new Queue<ObjectTreeNode>();
nodeQueue.Enqueue(apolloTree.RootNode);
while (nodeQueue.Any())
{
    var node = nodeQueue.Dequeue();
    Console.WriteLine("{0} = {1}", node.ToEdgePath().ToString(), node.Value);
    foreach (var childNode in node.ChildNodes)
    {
        nodeQueue.Enqueue(childNode);
    }
}
```

This example will write the following output:

```
<root> = Person("Apollo")
<root>.Name = Apollo
<root>.BestFriend = Person("Robert")
<root>.BestFriend.Name = Robert
<root>.BestFriend.BestFriend = Person("Clubber")
<root>.BestFriend.BestFriend.Name = Clubber
<root>.BestFriend.BestFriend.BestFriend =
```


### Object Comparison

This project includes a class for comparing two objects recursively.

```c#
var comparer = new ObjectTreeEqualityComparer();
var differences = comparer.FindDifferences(new { a = 1, b = 2 }, new { a = 2, b = 2});
```

That comparison will give you a Difference object like this:

```
<root>.a: expected value "1" but was "2".
```


### Code Generation

This project includes a class for generating C# code from an object tree.

For an example, let's use that Person class again:

```c#
public class Person
{
    public string Name { get; set; }
}

```

We can use this to generate C# code for a very basic person:

```c#
var generator = new CSharpObjectCodeGenerator();
string code = generator.GenerateCode(new Person { Name = "Apollo" });
```

Which will give us basically what we passed in as a string:

```c#
@"new Person()
{
	Name = "Apollo",
}"
```

So you might be asking, what is the point of generating the same code that I passed in?  You're right, what we did in that example was pretty stupid.  But the key to making this not pointless is that you can generate C# code from any object.

So for example, maybe we are reading these Person objects from a database or parsing them from some type of file.  Here's an example that generates the same output, but uses an XML string as input instead:

```c#
var doc = XDocument.Parse(@"<Person><Name>Apollo</Name></Person>");
var person = new Person { Name = doc.Root.Element("Name").Value };
var code = new CSharpObjectCodeGenerator().GenerateCode(person);
```

My primary use case for generating code like this is to create unit tests.  You can first call your real method that connects to a database or some other external resource, and then save the results as C# code.  Then when you create a unit test, you can use that code as mock input to the class or method that you want to test.

The current version of the C# code generator is still fairly experimental.  It works best on entities with a parameterless constructor and properties with public getters and setters.


## NUnit Project
In addition to the core ObjectTree project, this repository also contains an NUnit project for assertions that compare object tree representations of two objects.

Here are some examples of using it:

```c#
ObjectTreeAssert.AreEqual(expected, actual);
Assert.That(actual, IsObjectTree.EqualTo(expected));
```

```c#
ObjectTreeAssert.AreNotEqual(expected, actual);
Assert.That(actual, IsObjectTree.NotEqualTo(expected));
Assert.That(actual, Is.Not.ObjectTreeEqualTo(expected));
```

You also have the option of passing in a filter to control which properties are included in the comparison.  The [LatticeUtils](https://github.com/dotlattice/LatticeUtils/) ReflectionUtils class works well with these filters for selecting properties to exclude:

```c#
ObjectTreeAssert.AreEqual(expected, actual, new ObjectTreeNodeFilter
{
    ExcludedProperties = new[] 
    {
        ReflectionUtils.Property<Person>(x => x.FullName)
    }
});
Assert.That(actual, IsObjectTree.EqualTo(expected).WithFilter(new ObjectTreeNodeFilter
{
    ExcludedProperties = new[] 
    { 
        ReflectionUtils.Property<Person>(x => x.FullName)
    }
}));
```

## Todo

Right now, this project is still on a prerelease version.  The main thing left in the project is to decide if the code generation and tree comparison classes should remain in this class or be split off into separate projects.

And the other big thing that needs to be completed is this documentation.  At some point I'm hoping to add information about the following:

* Circular reference handling
* Node filtering
* Architecture overview 
* Extensibility (SpawnStrategy classes)

