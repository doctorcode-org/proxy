using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DoctorProxy.Data
{
    public class TreeNode<T> : IEnumerable<TreeNode<T>>
    {
        public T Data { get; set; }
        public TreeNode<T> Parent { get; set; }
        public List<TreeNode<T>> Children { get; set; }

        public TreeNode(T data)
        {
            this.Data = data;
            this.Children = new List<TreeNode<T>>();
        }

        public TreeNode<T> AddChild(T child)
        {
            
            var childNode = Children.Where(c => c.Data.Equals(child)).FirstOrDefault();

            if (childNode == null)
            {
                childNode = new TreeNode<T>(child) { Parent = this };
                this.Children.Add(childNode);
                return childNode;
            }

            return childNode;
        }


        public IEnumerator<TreeNode<T>> GetEnumerator()
        {
            throw new NotImplementedException();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            throw new NotImplementedException();
        }
    }
}