import { TreeNode } from 'primeng/api';

export function toTreeNode<T>(
  item: T,
  label: (item: T) => string,
  key: (item: T) => string,
  selectable: boolean,
  children: TreeNode[] = null
): TreeNode {
  const parent = {
    label: label(item),
    data: item,
    selectable: selectable,
    children: children,
    leaf: children ? false : true,
    key: key(item),
  };

  if (children) {
    children.forEach(c => (c.parent = parent));
  }

  return parent;
}
export function toTreeNodes<T>(
  items: T[],
  label: (item: T) => string,
  key: (item: T) => string,
  selectable: boolean,
  children: TreeNode[] = null
): TreeNode[] {
  return items.map(item => toTreeNode(item, label, key, selectable, children));
}
