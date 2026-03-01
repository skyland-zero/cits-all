export function filterTree(treeData: any[], inputValue: string, key: string) {
  const newTreeData: any[] = [];
  treeData.forEach(item => {
    // 使用类型断言确保 key 对应的值为字符串
    if ((item[key] as string).includes(inputValue)) {
      newTreeData.push(item);
    } else {
      // 递归处理子节点
      if (item.children?.length) {
        const filteredChildren = filterTree(item.children, inputValue, key);
        if (filteredChildren.length > 0) {
          newTreeData.push({ ...item, children: filteredChildren });
        }
      }
    }
  });
  return newTreeData;
};
