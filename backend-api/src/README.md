用户权限关联数据

用户-部门
用户-角色

缓存用户信息时分为2部分：  
（1）用户部门信息  
（2）用户权限信息（包括角色、菜单），主要用于前端获取菜单
（5）用户权限点信息（包括角色、权限点），主要用于后端判断角色和权限点

缓存动作：  
（1）缓存部门信息时，key为{userid}_dept，tag设置为dept_{deptid}  
（2）缓存权限信息时，key为{userid}_role，tag设置为role_{roleid},有多个role时设置多个tag

信息有变更时删除缓存：  
（1）用户部门变更：直接删除用户对应的部门缓存信息   
（2）用户角色变更：直接删除用户对应的权限缓存信息  
（3）用户关联的部门：部门信息变更后根据部门tag删除缓存  
（4）用户关联的角色：角色信息变更（包括重新配置角色权限）后根据角色tag删除缓存   
（5）用户关联的角色相关的菜单变更：查找到关联到菜单的角色，根据角色tag删除缓存

TODO:

1. 领域事件：[https://github.com/jbogard/MediatR] MediatR
2. 集成事件：  
   （1）MassTransit https://masstransit.io/    
   （2）CAP https://github.com/dotnetcore/CAP
3. 对象映射：Mapster https://github.com/MapsterMapper/Mapster

1.密码错误多次后锁定 


 
