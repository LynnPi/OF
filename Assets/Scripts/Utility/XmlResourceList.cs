using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;

public class XmlResourceList {
    public class Item {
        public string Path;
        public int Version;
        public string Md5;
        public bool Deleted;
        public int Size;
    }

    private Dictionary<string,Item> ItemDir_ = new Dictionary< string, Item >();

    public void Clear() {
        ItemDir_.Clear();
    }

    public void AddToItems( Item item ) {
        ItemDir_[item.Path.ToLower()] = item;
    }

    public void ReadFromXml( string xmlContent ) {
        XmlDocument doc = new XmlDocument();
        doc.LoadXml( xmlContent );
        Item[] items = GetItemsFromXml( doc );

        ItemDir_.Clear();
        foreach ( var item in items ) {
            ItemDir_[ item.Path.ToLower() ] = item;
        }
    }

    public Item GetItem( string path ) {
        Item item = null;
        ItemDir_.TryGetValue( path.ToLower(), out item );
        return item;
    }

    public Item[] GetAllItems(){
        return ItemDir_.Values.ToArray();
    }

    public int GetCount(){
        return ItemDir_.Count;
    }

    /// <summary>
    /// 根据文件扩展名找对应的Item
    /// </summary>
    /// <param name="ext"></param>
    /// <returns></returns>
    public Item[] FindItem( string ext ){
        return ItemDir_.Values.Where( item => item.Path.EndsWith( ext ) && !item.Deleted ).ToArray();
    }

    public static Item[] GetItemsFromXml( XmlDocument doc ) {
        XmlElement rootNode = doc.DocumentElement;
        XmlNodeList nodes = rootNode.GetElementsByTagName( "Item" );
        List<Item> items = new List<Item>();
        foreach ( XmlNode node in nodes ) {
            Item item = new Item();
            item.Deleted = bool.Parse( node.Attributes[ "Deleted" ].Value );
            item.Path = node.Attributes[ "Path" ].Value;
            item.Md5 = node.Attributes[ "Md5" ].Value;
            item.Size = int.Parse( node.Attributes[ "Size" ].Value );
            item.Version = int.Parse( node.Attributes[ "Version" ].Value );
            items.Add( item );
        }
        return items.ToArray();
    }

    public void Save( string filePath ) {
        XmlDocument doc = new XmlDocument();
        XmlDeclaration declare = doc.CreateXmlDeclaration( "1.0", "utf-8", "yes" );
        doc.AppendChild( declare );
        XmlElement rootElement = doc.CreateElement( "FileInfo" );
        doc.AppendChild( rootElement );

        foreach ( var item in this.ItemDir_.Values ) {
            XmlElement e = doc.CreateElement( "Item" );
            e.SetAttribute( "Path", item.Path );
            e.SetAttribute( "Size", item.Size.ToString() );
            e.SetAttribute( "Version", item.Version.ToString() );
            e.SetAttribute( "Md5", item.Md5 );
            e.SetAttribute( "Deleted", item.Deleted.ToString() );
            rootElement.AppendChild( e );
        }
        using ( XmlWriter xw = new XmlTextWriter( filePath, new UTF8Encoding( false ) ) ) {
            doc.Save(xw);
            xw.Flush();              
        }
    }
}
