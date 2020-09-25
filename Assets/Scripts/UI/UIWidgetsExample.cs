using System.Collections.Generic;
    using Unity.UIWidgets.animation;
    using Unity.UIWidgets.engine;
    using Unity.UIWidgets.foundation;
    using Unity.UIWidgets.material;
    using Unity.UIWidgets.painting;
    using Unity.UIWidgets.ui;
    using Unity.UIWidgets.widgets;
    using UnityEngine;
    using FontStyle = Unity.UIWidgets.ui.FontStyle;
    
    namespace UIWidgetsSample {
        public class UIWidgetsExample : UIWidgetsPanel {
            protected override void OnEnable() {
                // if you want to use your own font or font icons.   
                // FontManager.instance.addFont(Resources.Load<Font>(path: "path to your font"), "font family name");
    
                // load custom font with weight & style. The font weight & style corresponds to fontWeight, fontStyle of 
                // a TextStyle object
                // FontManager.instance.addFont(Resources.Load<Font>(path: "path to your font"), "Roboto", FontWeight.w500, 
                //    FontStyle.italic);
    
                // add material icons, familyName must be "Material Icons"
                // FontManager.instance.addFont(Resources.Load<Font>(path: "path to material icons"), "Material Icons");
                
                base.OnEnable();
            }
    
            protected override Widget createWidget() {
                return new WidgetsApp(
                    home: new ExampleApp(),
                    pageRouteBuilder: (RouteSettings settings, WidgetBuilder builder) =>
                        new PageRouteBuilder(
                            settings: settings,
                            pageBuilder: (BuildContext context, Animation<float> animation,
                                Animation<float> secondaryAnimation) => builder(context)
                        )
                );
            }
    
            class ExampleApp : StatefulWidget {
                public ExampleApp(Key key = null) : base(key) {
                }
    
                public override State createState() {
                    return new ExampleState();
                }
            }
    
            class ExampleState : State<ExampleApp> {
                int counter = 0;
    
                public override Widget build(BuildContext context) {
                    return new Scaffold(appBar: new AppBar(title: new Text("dd")));
                }
            }
        }
    }