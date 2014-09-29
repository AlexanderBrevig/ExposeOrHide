ExposeOrHide
============

An API for exposing or hiding properties on a model. Useful for small footprint JSON REST services


##An example

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


    var dyna = model.Hide(hide => hide.Testing)
                    .HideMember<Member>(member => member.Member, hide => hide.Member1)
                    .ExposeMember<Obj>(member => member.Objects, hide => hide.Hi)
                    .ToDynamic();

    string hi = dyna.Objects[0].Hi;
    //string error = dyn.Member.Member1; // not accessible

##Remember to install

    PM> Install-Package ExposeOrHide
