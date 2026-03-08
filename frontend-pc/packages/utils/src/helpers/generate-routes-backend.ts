import type { RouteRecordRaw } from 'vue-router';

import type {
  ComponentRecordType,
  GenerateMenuAndRoutesOptions,
  RouteRecordStringComponent,
} from '@vben-core/typings';

/**
 * 动态生成路由 - 后端方式
 */
async function generateRoutesByBackend(
  options: GenerateMenuAndRoutesOptions,
): Promise<RouteRecordRaw[]> {
  const { fetchMenuListAsync, layoutMap = {}, pageMap = {} } = options;

  try {
    const menuRoutes = await fetchMenuListAsync?.();
    if (!menuRoutes) {
      return [];
    }

    const normalizePageMap: ComponentRecordType = {};

    for (const [key, value] of Object.entries(pageMap)) {
      normalizePageMap[normalizeViewPath(key)] = value;
    }

    const routes = convertRoutes(menuRoutes, layoutMap, normalizePageMap);

    return routes;
  } catch (error) {
    console.error(error);
    throw error;
  }
}

function convertRoutes(
  routes: RouteRecordStringComponent[],
  layoutMap: ComponentRecordType,
  pageMap: ComponentRecordType,
  ancestors: string[] = [],
): RouteRecordRaw[] {
  return routes.map((node) => {
    const route = { ...node } as unknown as RouteRecordRaw;
    let { component, name } = node;

    if (!name) {
      console.error('route name is required', route);
    } else if (ancestors.includes(name as string)) {
      // 如果名称与祖先重复，则添加后缀以避免 vue-router 报错
      // 这种情况通常发生在后端数据配置不当时（例如分组和子页面同名）
      name = `${name as string}_${ancestors.length}`;
      route.name = name;
    }

    const currentAncestors = name ? [...ancestors, name as string] : ancestors;

    // layout转换
    if (component && layoutMap[component]) {
      route.component = layoutMap[component];
      // 页面组件转换
    } else if (component) {
      const normalizePath = normalizeViewPath(component);
      const pageKey = normalizePath.endsWith('.vue')
        ? normalizePath
        : `${normalizePath}.vue`;
      if (pageMap[pageKey]) {
        route.component = pageMap[pageKey];
      } else {
        console.error(`route component is invalid: ${pageKey}`, route);
        route.component = pageMap['/_core/fallback/not-found.vue'];
      }
    }

    if (node.children && node.children.length > 0) {
      route.children = convertRoutes(
        node.children,
        layoutMap,
        pageMap,
        currentAncestors,
      );
    }

    return route;
  });
}

function normalizeViewPath(path: string): string {
  // 去除相对路径前缀
  const normalizedPath = path.replace(/^(\.\/|\.\.\/)+/, '');

  // 确保路径以 '/' 开头
  const viewPath = normalizedPath.startsWith('/')
    ? normalizedPath
    : `/${normalizedPath}`;

  // 这里耦合了vben-admin的目录结构
  return viewPath.replace(/^\/views/, '');
}
export { generateRoutesByBackend };
