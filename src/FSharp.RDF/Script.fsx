#r "../../packages/FSharpx.Core/lib/40/FSharpx.Core.dll"
#r "../../packages/Unquote/lib/net40/Unquote.dll"
#r "../../packages/json-ld.net/lib/net40-Client/JsonLD.dll"
#I "../../packages/Newtonsoft.Json/lib/net40/"
#r "../../packages/HtmlAgilityPack/lib/Net40/HtmlAgilityPack.dll"
#r "../../packages/Newtonsoft.Json/lib/net40/Newtonsoft.Json.dll"
#I "../../lib/owlapi.net_release_1_0_0/"
#r "../../lib/owlapi.net_release_1_0_0/IKVM.OpenJDK.Core.dll"
#r "../../lib/owlapi.net_release_1_0_0/owlapi.dll"
#r "../../lib/owlapi.net_release_1_0_0/Cognitum.OwlApi.Net.ReasonerInterface.dll"
#r "../../lib/owlapi.net_release_1_0_0/Reasoners/Pellet/Cognitum.OwlApi.Net.Pellet.dll"
#I "../../lib/owlapi.net_release_1_0_0/Reasoners/Pellet"
#r "../../lib/owlapi.net_release_1_0_0/Reasoners/Pellet/pellet.dll"
#r "../../packages/dotNetRDF/lib/net40/dotNetRDF.dll"
#r "../../packages/VDS.Common/lib/net40-client/VDS.Common.dll"
#r "../../bin/FSharp.RDF.dll"

open FSharp.RDF
open System.IO
open Swensen.Unquote
open FSharp.RDF.Ontology
open FSharp.RDF.Assertion


let g = Graph.empty !!"http://lol" []
let sb = System.Text.StringBuilder()

Graph.writeTtl (toString sb) g
printf "%s" (string sb)

[<Literal>]
let pizzaF = __SOURCE_DIRECTORY__ + "/Pizza.ttl"


let o = Ontology.loadFile "/Users/ryanroberts/code/FSharp.RDF/src/FSharp.RDF/pizza.ttl"

do
  let {
    Ontology = o2
    Reasoner = r
    DataFactory =d
    } = o
  let clss = o2.getClassesInSignature()

  printf "%A" clss
Ontology.cls o "http://www.w3.org/2002/07/owl#Thing"

type pizza = FSharp.RDF.OntologyProvider<pizzaF,"http://www.w3.org/2002/07/owl#Thing">

type thing = pizza.``http://www.w3.org/2002/07/owl#Thing``
