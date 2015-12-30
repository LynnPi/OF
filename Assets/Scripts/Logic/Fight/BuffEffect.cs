using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

public class BuffEffect {
    public static readonly BuffEffect Instance = new BuffEffect();
    public BuffEffect() {

    }

    public int CalcBuffEffect( ShipEntity shipEntity, int buffType ) {
        switch( buffType ) {
                //在此处添加方法与类型的对应关系
            default: return 0;
            
        }
    }

    //在下面添加类型对应的方法
}
