import { LoginModuleConfiguration } from '../app/login/models/login-module-configuration';
import { RealtimeModuleConfiguration } from '../app/realtime/models/realtime-module-configuration';
import { TodoModuleConfiguration } from '../app/todo/models/todo-module-configuration';

export const environment = {
  production: true,
};

export class LoginConfiguration extends LoginModuleConfiguration {
  authorityUrl = 'https://dotnetsummit-spring-2018-services-identity.azurewebsites.net';
  clientId = 'guiclient';
  clientSecret = 'guisecret';
  loginUrl = '/login';
  loginRedirectUrl = '/todo';
}

export class TodoConfiguration extends TodoModuleConfiguration {
  apiUrl = 'https://dotnetsummit-spring-2018-services-api.azurewebsites.net/api';
}

export class RealtimeConfiguration extends RealtimeModuleConfiguration {
  hubUrl = 'https://dotnetsummit-spring-2018-services-push.azurewebsites.net';
}
