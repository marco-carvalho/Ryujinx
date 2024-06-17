using System.Collections.Generic;
using System.IO;

namespace Ryujinx.HLE.HOS.Diagnostics.Demangler.Ast
{
    public class PackedTemplateParameter : NodeArray
    {
        public PackedTemplateParameter(List<BaseNode> nodes) : base(nodes, NodeType.PackedTemplateParameter) { }

        public override void PrintLeft(TextWriter writer)
        {
            for (int i = 0; i < Nodes.Count; i++)
            {
                BaseNode node = Nodes[i];
                node.PrintLeft(writer);
            }
        }

        public override void PrintRight(TextWriter writer)
        {
            for (int i = 0; i < Nodes.Count; i++)
            {
                BaseNode node = Nodes[i];
                node.PrintLeft(writer);
            }
        }

        public override bool HasRightPart()
        {
            for (int i = 0; i < Nodes.Count; i++)
            {
                BaseNode node = Nodes[i];
                if (node.HasRightPart())
                {
                    return true;
                }
            }

            return false;
        }
    }
}
