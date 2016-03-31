using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace percip.io
{
    public interface IDataSaver
    {
        void Save<T>(string filename, T obj) where T : class;
        T Load<T>(string filename) where T : class;
    }
}
