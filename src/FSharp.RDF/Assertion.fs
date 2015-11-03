namespace FSharp.RDF

module Assertion =
  open VDS.RDF
  open VDS.RDF.Writing
  open VDS.RDF.Writing.Formatting
  open FSharpx
  open FSharp.RDF

  module Assert =
    let toXmlLiteral (g:IGraph) (s:string) =
      VDS.RDF.Nodes.StringNode(g,s,System.Uri("rdf:XMLLiteral"))

    let toVDSNode g n : INode =
      match n with
      | (Literal.String s) -> s.ToLiteral g :> INode
      | (Literal.DateTimeOffset d) -> d.ToLiteral g :> INode
      | (Literal.XMLLiteral x) -> x |> toXmlLiteral g :> INode

    //Dotnetrdf doesn't accept a uri as a string, only qnames
    //probably because System.Uri used to explode if you tried
    let private uriFromPossibleQname (g : IGraph) (u : System.Uri) =
      match u.Scheme with
      | "http" | "https" | "file" -> g.CreateUriNode u
      | _ -> g.CreateUriNode(string u)


    let rec private triple (FSharp.RDF.Graph g) (S s, p, o) =
      let rec assrtTriple (s : INode) (p, o) = seq {
        match p,o with
        | (P p, O(Node.Blank(Blank.Blank(xst')), _)) -> //Blank node
          let b = g.CreateBlankNode()
          yield Triple(s, uriFromPossibleQname g (Uri.toSys p), b)
          for (p, o) in xst' do
            yield! assrtTriple b (p, o)
        | (P p, O(Node.Uri(o), xr)) -> //Dependent resources
          let o = uriFromPossibleQname g (Uri.toSys o)
          yield Triple(s, uriFromPossibleQname g (Uri.toSys p), o)
          yield! triples (Graph g) xr.Value
        | (P p, O(Node.Literal l, _)) -> //Literal
          yield Triple(s, uriFromPossibleQname g (Uri.toSys p), toVDSNode g l)
        }
      let s = uriFromPossibleQname g (Uri.toSys s)
      assrtTriple s (p, o)

    and private asrtTriples (Graph g) tx = seq {
      for t in tx do
        for t in triple (Graph g) t do
          g.Assert t |> ignore
          yield t
      }

    and graph g tx =
      triples g tx |> Seq.iter (fun _ -> ())
      g
    and triples g (xr:Resource seq) = Seq.collect Resource.asTriples xr |> asrtTriples g

    let ttl() = CompressingTurtleWriter() :> IRdfWriter

    let format (f : unit -> IRdfWriter) (tw : System.IO.TextWriter) o =
      match o with
      | FSharp.RDF.Graph g ->
        (f()).Save(g, tw)
        o

  module xsd =
    let string s = Node.Literal(Literal.String s)
    let datetime d = Node.Literal(Literal.DateTimeOffset d)
    let xmlliteral x = Node.Literal(Literal.XMLLiteral x)

  let uri u = (Uri.Sys(System.Uri u))
  let (!!) = uri
  let inline (^^) t f = f t

  module rdf =
    let objectProperty p o = ((P p), O(Node.Uri o, lazy []))
    let one p o xst = ((P p), O(Node.Uri o, lazy [ R(S o, xst) ]))
    let a t = objectProperty (uri "rdf:type") t
    let dataProperty p o = ((P p), O(o, lazy []))
    let blank p xst = (P p, O(Node.Blank(Blank.Blank(xst)), lazy []))
    let resource s xst = R(S s, xst)
    let triple s (p, o) = (S s, P p, O o)

  module rdfs =
    open rdf
    let subClassOf t = objectProperty (uri "rdfs:subClassOf") t

  module owl =
    open rdf
    let individual s xt xst =
      R(S s,
        [ yield rdf.a !!"owl:NamedIndividual"
          for t in xt -> a t]
        @ xst)

    let cls s xt xst =
      R(S s,
        [ yield rdf.a !!"owl:Class"
          for t in xt -> objectProperty (uri "rdfs:subClassOf")  t ]
        @ xst)

    let pun s xt xst =
      R(S s,
           [ yield rdf.a !!"owl:NamedIndividual"
             yield rdf.a !!"owl:Class"
             for t in xt -> objectProperty (uri "rdfs:subClassOf")  t ]
           @ xst)
