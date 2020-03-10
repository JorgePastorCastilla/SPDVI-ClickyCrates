using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Scripts.Modelos
{
    [Serializable]
    class GameSerializable
    {
        public int Id;
        public string PlayerId;
        public string Start;
        public string Final;
        public string Difficulty;
        public int Points;
    }
}
