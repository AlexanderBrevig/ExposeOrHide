ExposeOrHide
============

An API for exposing or hiding properties on a model. Useful for small footprint JSON REST services


##An example

Given this model:

    var model = new Test() {
        Trial = "test",
        Testing = "testing",
        Member = new Member() {
            Member1 = "member1",
            Member2 = "member2"
        },
        Objects = new List<Obj>() {
            new Obj() {Hi ="there", Hello ="to you" },
            new Obj() {Hi ="here" , Hello ="to me"},
        }
    };

You could get a dynamic object like this:

    var dyna = model.Hide(hide => hide.Testing)
                    .HideMember<Member>(member => member.Member, hide => hide.Member1)
                    .ExposeMember<Obj>(member => member.Objects, hide => hide.Hi)
                    .ToDynamic();

    string hi = dyna.Objects[0].Hi;
    //string error = dyn.Member.Member1; // not accessible
    
If you serialize to JSON, this is what you'd get:
    
    {
        //No Testing propery, it’s hidden
        Trial: "test",
        Objects: [ //No Hello property because only Hi were white-listed (for all in list)
            { 
                Hi: "there"
            },{
                Hi: "here"
            }
        ],
        Member: {
            //No Member1 property, it’s hidden
            Member2: "member2"
        }
    }


##Remember to install

    PM> Install-Package ExposeOrHide
    
    
