# RuntimeSerializer
Easy to use Runtime Serialization library for C#. At runtime, it creates a new assembly with a new derived type implementing serialization.

Sometimes you might get some objects  which are not marked as serializable but you want to serialize it. But you cannot do it at compile time as the source may not be available for you to edit(eg: .Net defined controls like Buttons, Forms etc or other class defined in 3rd party libraries). This library helps you to create a dynamic object at runtime itself which gives you a serializable version of that object.

The idea is to create a new class at runtime, which derives from existing object's class and implements serialization as well. We then create an object of the new class and copy all items from exiting object to the new object.

The new object now has all values in the existing object and it is serializable as well.

#Usage
Just add RuntimeSerializer.dll and call RuntimeSerializer.GenerateSerializableObject API.
Pass in existing object as paramater and it returns  the new serializable object

For more info on how to use the library, please check the Client.Program file

#Restrictions
As of now it works for only those objects which have a default constructir and the class has to be defined as public

