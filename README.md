# LatticeObjectTree [![Build status](https://ci.appveyor.com/api/projects/status/uptdctnkaitlyslg)](https://ci.appveyor.com/project/dotlattice/latticeobjecttree)

LatticeObjectTree is a .NET library for working with a tree of objects.  The primary use that this library was designed for is comparing two objects to each other recursively, but it can also be used for many other purposes.

## Installation

There are several ways to install this library:

* Install the [NuGet package](https://www.nuget.org/packages/LatticeObjectTree/)
* Download the assembly from the [latest release](https://github.com/dotlattice/LatticeObjectTree/releases/latest) and install it manually
* Copy the parts you want into your own project

This entire library is released into the [public domain](https://github.com/dotlattice/LatticeObjectTree/blob/master/LICENSE).  So you can copy anything from this library into your own project without having to worry about attribution or any of that stuff.

There is also a separate [NuGet package](https://www.nuget.org/packages/LatticeObjectTree.NUnit/) for using the object comparisons in NUnit test assertions.  For more on that see the [NUnit](#nunit) section.

## Object Comparison

The primary function of this library is to support recursive object comparisons.

Here's a basic example of comparing two anonymous objects:

```c#
var comparer = new ObjectTreeEqualityComparer();
var differences = comparer.FindDifferences(new { a = 1, b = 2 }, new { a = 2, b = 2});
```

That comparison will give you a list of differences.  For this example, that looks like this:

```
<root>.a: expected value "1" but was "2".
```

By default, these comparisons iterate through the public properties and fields of objects and the elements of enumerable types.  It also has built-in cycle detection, so you can compare objects even if there are circular references.  But if the default comparisons don't work for you, take a look at the [extensibility](#extensibility) section for ways to customize it.


## Filtering

There are often times where you want to compare two objects based on only some of their properties.  This is supported through filter objects.

The default filter can be used to exclude properties in several ways.  The simplest is excluding properties by name:

```c#
new ObjectTreeNodeFilter
{
    ExcludedPropertyNames = new[] { "Name" }
}
```

Once you've created a filter, you can pass it into most of the methods for working with object trees as optional parameter.  For example, to use a filter with the ObjectTreeEqualityComparer:

```c#
var comparer = new ObjectTreeEqualityComparer();
var differences = comparer.FindDifferences(new { a = 1, b = 2 }, new { a = 2, b = 2}, new ObjectTreeNodeFilter
{
    ExcludedPropertyNames = new[] { "a" }
});
```

In this example, the anonymous objects will be compared and no differences will be found because the "a" properties are excluded.

There are also more ways to filter than by just property name.  In real life,  you'll often want to exclude a specific property from a specific type.  That can be done like using the "ExcludedProperties" filter option:

```c#
new ObjectTreeNodeFilter
{
    ExcludedProperties = new[] 
    { 
        typeof(Person).GetProperty("Name")
    }
})
```
 
You can also use an expression-based utility like the [LatticeUtils](https://github.com/dotlattice/LatticeUtils/) ReflectionUtils class for selecting these properties in a way that provides compile-time checking:

```c#
new ObjectTreeNodeFilter
{
    ExcludedProperties = new[] 
    { 
        ReflectionUtils.Property<Person>(x => x.FullName)
    }
})
```

If you need even more control, there is an "ExcludedPropertyPredicates" option that uses predicate functions.  Here's an example that filters out any properties with an "a" in its name:

```c#
new ObjectTreeNodeFilter
{
    ExcludedPropertyPredicates = new Func<PropertyInfo, bool>[] 
    { 
        (propertyInfo) => propertyInfo.Name.Contains("a"),
    }
});
```

And the final option that gives you the most control is the "ExcludedNodePredicates" option.  This is a list of predicates that work on nodes (instead of properties), so you can base your filter on the context of an object in the tree.  Here's an example that limits the depth of the tree:

```c#
new ObjectTreeNodeFilter
{
    ExcludedNodePredicates = new Func<ObjectTreeNode, bool>[] 
    { 
        (node) => node.ToEdgePath().Edges.Count > 3
    }
});
```

And if you still can't create the filter you want with those options, then you can write your own completely custom filter.  Everything in the ObjectTree library works with the IObjectTreeNodeFilter interface, so you can implement that in your own class to create any kind of filter:

```c#
public interface IObjectTreeNodeFilter
{
    IEnumerable<ObjectTreeNode> Apply(IEnumerable<ObjectTreeNode> nodes);
}
```

## Object Tree

While the primary use case of this library is for comparing objects, you can also use the ObjectTree classes directly for all kinds of purposes.

Here's a very basic example of creating an ObjectTree directly:

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

```
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
```

There are two main parts to an object tree:

* Nodes - the values (boxes in the diagram).  In this example, there is a node for each Person object and for each "Name" string.
* Paths - the connections between nodes (lines in the diagram).  In this example, all of the paths are properties (the "Name" property and the "BestFriend" property).

To use the tree, you can iterate through these nodes and the paths that connect them:

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

## Cycle Detection

The ObjectTree also includes handling for circular references by default.

Let's take the above example and add a circular references:

```c#
var clubber = new Person
{
    Name = "Clubber"
};
var apollo = new Person
{
    Name = "Apollo",
    BestFriend =  new Person 
    { 
        Name = "Robert",
        BestFriend = clubber,
    },
};
clubber.BestFriend = apollo;

var apolloTree = ObjectTree.Create(apollo);
```

With this circular reference, our tree looks like this:

```
         +---------------+                        
         |Person (Apollo)| <---------------------+
         +-----+---------+                       |
               |                                 |
       Name    |   BestFriend                    |
     +---------+----------+                      |
     |                    |                      |
+----+---+        +-------+-------+              |
|"Apollo"|        |Person (Robert)|              |
+--------+        +-------+-------+              |
                          |                      |
                  Name    |   BestFriend         |
                +---------+----------+           |
                |                    |           |
            +---+----+       +-------+--------+  |
            |"Robert"|       |Person (Clubber)|  |
            +--------+       +-------+--------+  |
                                     |           |
                             Name    |           |
                           +---------+-----------+
                           |           BestFriend 
                      +----+----+                 
                      |"Clubber"|                 
                      +---------+
```

Because of that cycle, you would get into an infinite loop if you tried to iterate through this tree without any special handling.

So what do we do to avoid that?  When a reference to an object that is already in the tree is found, a different type of object node is created for it.  There are two special things about this node:

* The "ChildNodes" property is always an empty enumerable
* The "OriginalNode" property is set to the first node that was created for the value

So because its "ChildNodes" property of the duplicate node will be empty, you won't get into an infinite loop if you try to iterate through the tree.  And because the "OriginalNode" property is set, you can detect that this node is a duplicate and do any special handling you might need for a duplicate value.


## Extensibility

The goal in designing this library was to make it very simple to use out of the box while still making it very customizable.  So there are several extension points for replacing the default logic.

### Spawn Strategies

An ObjectTree node is in charge of creating its own child elements, and the ObjectTreeNode class does this using an implementation of the IObjectTreeSpawnStrategy interface.

```c#
public interface IObjectTreeSpawnStrategy
{
    IEnumerable<ObjectTreeNode> CreateChildNodes(ObjectTreeNode node, IObjectTreeSpawnStrategy spawnStrategyOverride = null);
}
```

This is the primary extension point in the library.  The default works great if your objects mostly consist of public properties or fields, but in other cases you may need to implement your own strategy for determining what the child nodes of an object are.

There are four spawn strategies included in the library by default:

* BasicObjectTreeSpawnStrategy
  * The most basic, core spawn strategy
  * Looks at the type of the node's value and creates child nodes based on the public properties and fields in that type.
  * Does not include any cycle detection or filtering support.
* DuplicateCheckingObjectTreeSpawnStrategy
	* The default strategy used by an ObjectTreeNode
	* Delegates the actual child node creation to another spawn strategy (BasicObjectTreeSpawnStrategy by default).
	* Stores a copy of every node in the tree as they are encountered for duplicate detection.
	* Creates a special node with no children when a duplicate value is encountered.
* FilteredObjectTreeSpawnStrategy
	* The default strategy used by an ObjectTreeNode if a filter is specified
	* Delegates the actual child node creation to another spawn strategy (DuplicateCheckingObjectTreeSpawnStrategy by default), then applies its filter to the results.
* EmptyObjectTreeSpawnStrategy
	* A special spawn strategy that always returns an empty list of nodes.
	* Used for duplicate nodes created by the DuplicateCheckingObjectTreeSpawnStrategy

If you need to implement your own spawn strategy, you can base it on one of these or create a completely independent one that implements the IObjectTreeSpawnStrategy interface.

### Filters

The filters in this library are all based on the IObjectTreeNodeFilter interface:

```c#
public interface IObjectTreeNodeFilter
{
    IEnumerable<ObjectTreeNode> Apply(IEnumerable<ObjectTreeNode> nodes);
}
```

The filter included in the library is the ObjectTreeNodeFilter class.  By default it accepts every node except the ones that you specifically exclude.  See the [Filtering](#filtering) section for more details.

But there are lots of other ways you could filter properties.  For example, you may want to exclude all nodes except the ones that you explicitly include.  To do something like that, you can implement the interface with your own class and use it anywhere that you could use the default filter.


### Edges

The edges in an object tree are the links between the object nodes.  The edges in an ObjectTree are implementations of the IObjectTreeEdge.

By default, an ObjectTree is constructed using the DefaultObjectTreeEdge for its edges.  This works great if all of your edges are either properties, fields, or indexes in some kind of enumerable.

An interface was used for these edges to account for the possibility of having some other kind of link between nodes.  So you can consider creating your own implementation of the IObjectTreeEdge interface if the default class doesn't have enough information in it for your purposes.


## NUnit
In addition to the core ObjectTree project, this repository also contains an NUnit project for writing assertions comparing object tree representations of two objects.

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

You can also use [filters](#filtering) in the assert methods:

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
