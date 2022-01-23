using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace PSH
{
    public class SHTools
    {
        public struct SHL0
        {
            public Vector3 l0; // l0r, l0g, l0b
            public override string ToString() => $"{l0}";
        }

        public struct SHL1
        {
            public Vector4 l1r; // l10r, l11r, l12r, l0r
            public Vector4 l1g; // l10g, l11g, l12g, l0g
            public Vector4 l1b; // l10b, l11b, l12b, l0b
            public override string ToString() => $"{l1r}, {l1g}, {l1b}";
        };

        public struct SHL2
        {
            public Vector4 l1r; // l10r, l11r, l12r, l0r
            public Vector4 l1g; // l10g, l11g, l12g, l0g
            public Vector4 l1b; // l10b, l11b, l12b, l0b
            public Vector4 l2r; // l20r, l21r, l22r, l23r
            public Vector4 l2g; // l20g, l21g, l22g, l23g
            public Vector4 l2b; // l20b, l21b, l22b, l23b
            public Vector4 l2c; // l24r, l24g, l24b, nil
            public override string ToString() => $"{l1r}, {l1g}, {l1b}, {l2r}, {l2g}, {l2b}, {l2c}";
        };
    }
}